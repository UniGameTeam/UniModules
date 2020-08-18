﻿namespace UniModules.UniGame.GoogleSpreadsheetsImporter.Editor.SheetsImporter
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Core.EditorTools.Editor.AssetOperations;
    using Extensions;
    using GoogleSpreadsheets.Editor.SheetsImporter;
    using GoogleSpreadsheets.Runtime.Attributes;
    using UniGreenModules.UniCore.EditorTools.Editor;
    using UniGreenModules.UniCore.Runtime.ReflectionUtils;
    using UniGreenModules.UniGame.Core.Runtime.Extension;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class AssetSheetDataProcessor
    {
        private static SheetSyncValue _dummyItem = new SheetSyncValue(string.Empty);

        public SheetSyncValue CreateSyncItem(object source)
        {
            return source == null ? _dummyItem : CreateSyncItem(source.GetType());
        }
        
        public SheetSyncValue CreateSyncItem(Type type)
        {
            var sheetName    = type.Name;
            var useAllFields = true;

            var sheetItemAttribute = type.GetCustomAttribute<SpreadsheetTargetAttribute>();
            if (sheetItemAttribute != null) {
                useAllFields = sheetItemAttribute.SyncAllFields;
                sheetName    = sheetItemAttribute.SheetName;
            }

            var result = new SheetSyncValue(sheetName);

            var fields = LoadSyncFieldsData(type, useAllFields);
            result.fields = fields.ToArray();
            
            result.keyField = result.fields.FirstOrDefault(x => x.isKeyField);
            
            return result;

        }

        /// <summary>
        /// Sync folder assets by spreadsheet data
        /// </summary>
        /// <param name="filterType"></param>
        /// <param name="folder"></param>
        /// <param name="createMissing">if true - create missing assets</param>
        /// <param name="spreadsheetData"></param>
        /// <param name="maxItems"></param>
        /// <param name="overrideSheetId"></param>
        /// <returns></returns>
        public List<Object> SyncFolderAssets(
            Type filterType, 
            string folder,
            bool createMissing, 
            SpreadsheetData spreadsheetData,
            int maxItems = -1,
            string overrideSheetId = "")
        {
            if (!filterType.IsScriptableObject() && !filterType.IsComponent()) {
                Debug.LogError($"SyncFolderAssets: BAD target type {filterType}");
                return null;
            }
            
            var assets = AssetEditorTools.GetAssets<Object>(filterType, folder);
            var result = SyncFolderAssets(
                filterType, 
                folder,
                spreadsheetData,
                assets.ToArray(), 
                createMissing,maxItems,overrideSheetId );
            return result;
        }

        /// <summary>
        /// Sync folder assets by spreadsheet data
        /// </summary>
        /// <param name="filterType"></param>
        /// <param name="assets"></param>
        /// <param name="folder"></param>
        /// <param name="createMissing">if true - create missing assets</param>
        /// <param name="spreadsheetData"></param>
        /// <param name="maxItemsCount"></param>
        /// <param name="overrideSheetId">force override target sheet id</param>
        /// <returns></returns>
        public List<Object> SyncFolderAssets(
            Type filterType, 
            string folder,
            SpreadsheetData spreadsheetData,
            Object[] assets = null,
            bool createMissing = true, 
            int maxItemsCount = -1,
            string overrideSheetId = "")
        {
            var result = assets != null ? 
                new List<Object>(assets) : 
                new List<Object>();
            
            if (!filterType.IsScriptableObject() && !filterType.IsComponent()) {
                Debug.LogError($"SyncFolderAssets: BAD target type {filterType}");
                return result;
            }

            var syncScheme = filterType.ToSpreadsheetSyncedItem();
            
            var sheetId = string.IsNullOrEmpty(overrideSheetId) ?
                syncScheme.sheetId : 
                overrideSheetId;
            
            var sheet      = spreadsheetData[sheetId];
            if (sheet == null) {
                Debug.LogWarning($"{nameof(AssetSheetDataProcessor)} Missing Sheet with name {sheetId}");
                return result;
            }

            var keyField = syncScheme.keyField;

            if (keyField == null) {
                Debug.LogWarning($"{nameof(AssetSheetDataProcessor)} Key field missing sheet = {sheetId}");
                return result;
            }
            
            var keysId   = keyField.sheetValueField;
            var column     = sheet.GetColumn(keysId);
            if (column == null) {
                Debug.LogWarning($"{nameof(AssetSheetDataProcessor)} Keys line missing with id = {keysId}");
                return result;
            }
            
            foreach (var importedAsset in 
                ApplyAssets(
                    filterType,
                    sheetId,
                    folder,
                    syncScheme,
                    spreadsheetData,
                    sheet.GetColumnValues(keysId).ToArray(),
                    assets,maxItemsCount,createMissing)) {
                result.Add(importedAsset);
            }

            return result;
        }

        public IEnumerable<Object> ApplyAssets(
            Type filterType,
            string sheetId,
            string folder,
            SheetSyncValue syncScheme,
            SpreadsheetData spreadsheetData,
            object[] keys,
            Object[] assets = null,
            int count = -1,
            bool createMissing = true,
            string keyFieldName = "")
        {
            count = count < 0 ? keys.Length : count;
            count = Math.Min(keys.Length, count);
            
            var keyField = string.IsNullOrEmpty(keyFieldName) ?
                syncScheme.keyField :
                syncScheme.GetFieldBySheetFieldName(keyFieldName);
            
            try {
                for (var i = 0; i < count; i++) {
                    
                    var keyValue = keys[i];
                    var key      = keyValue as string;
                    var targetAsset = assets?.
                        FirstOrDefault(x => string.Equals(keyField.
                                GetValue(x).ToString(), key, StringComparison.OrdinalIgnoreCase));

                    //create asset if missing
                    if (targetAsset == null) {
                        //skip asset creation step
                        if (createMissing == false)
                            continue;

                        targetAsset = filterType.CreateAsset();
                        targetAsset.SaveAsset($"{filterType.Name}_{i+1}", folder,false);
                        Debug.Log($"Create Asset [{targetAsset}] for path {folder}", targetAsset);
                    }

                    AssetEditorTools.ShowProgress(new ProgressData() {
                        IsDone = false,
                        Progress = i / (float)count,
                        Content = $"{i}:{count}  {targetAsset.name}",
                        Title = "Spreadsheet Importing"
                    });
                    
                    ApplyData(targetAsset,keyField, key,  syncScheme, spreadsheetData[sheetId]);

                    yield return targetAsset;
                }
            }
            finally {
                AssetEditorTools.ShowProgress(new ProgressData() {
                    IsDone = true,
                });
                AssetDatabase.SaveAssets();
            }

        }
        
        public object ApplyData(object source,SheetSyncValue syncScheme, DataRow row)
        {
            var rowValues = row.ItemArray;
            var table = row.Table;
            for (var i = 0; i < rowValues.Length; i++) {
                var columnName = table.Columns[i].ColumnName;
                var itemField  = syncScheme.fields.
                    FirstOrDefault(x => SheetData.IsEquals(x.sheetValueField,columnName));

                if (itemField == null)
                    continue;

                var rowValue = rowValues[i];
                var resultValue = rowValue.ConvertType(itemField.targetType);

                itemField.ApplyValue(source, resultValue);
            }

            return source;
        }
        
        public object ApplyDataByAssetKey(object source,
            SheetSyncValue schema, 
            SpreadsheetData spreadsheetData,
            string sheetKey = "")
        {
            var keyField = string.IsNullOrEmpty(sheetKey) ? schema.keyField : 
                schema.GetFieldBySheetFieldName(sheetKey);
            var keyValue = keyField.GetValue(source);
            
            return ApplyData(source, keyValue, keyField.sheetValueField, schema, spreadsheetData[schema.sheetId]);
        }
        
        public SheetData UpdateSheetValue(object source, SheetData data, string sheetKeyField = "")
        {
            if (source == null)
                return data;
            var type = source.GetType();
            var syncScheme = type.ToSpreadsheetSyncedItem();

            var keyField = string.IsNullOrEmpty(sheetKeyField) ?
                syncScheme.keyField :
                syncScheme.GetFieldBySheetFieldName(sheetKeyField);
            
            if (keyField == null)
                return data;
            
            var keyValue = keyField.GetValue(source);
            
            return UpdateSheetValue(source,keyValue,keyField.sheetValueField,syncScheme,data);
        }
        
        public SheetData UpdateSheetValue(object source,object keyValue,string keyFieldId, SheetSyncValue schemaValue, SheetData data)
        {
            if (keyValue == null || source == null)
                return data;
            
            var row = data.GetRow(keyFieldId, keyValue) ?? data.CreateRow();
            
            var sheetFields = SelectSheetFields(schemaValue, data);

            var index = 0;
            foreach (var field in sheetFields) {
                var sourceValue = field?.GetValue(source);
                data.UpdateValue(row,index,sourceValue ?? string.Empty);
                index++;
            }

            return data;
        }

        public IEnumerable<SyncField> SelectSheetFields(SheetSyncValue schemaValue,SheetData data)
        {
            var columns = data.Columns;
            for (var i = 0; i < columns.Count; i++) {
                var column = columns[i];
                var field = schemaValue.fields.
                    FirstOrDefault(x => SheetData.
                        IsEquals(x.sheetValueField, column.ColumnName));
                if(field == null)
                    yield return null;
                yield return field;
            }
        }

        public object ApplyData(object source,object key, string sheetField,SheetSyncValue value, SheetData sheet)
        {
            return ApplyData(source,sheetField,key,value,sheet);
        }
        
        public object ApplyData(object source,string sheetKeyField,object key,SheetSyncValue value, SheetData sheet)
        {
            var result = source;
            if (sheet == null) {
                Debug.LogWarning($"ApplyData SheetSyncValue : for {value.target} Key Field pr SheetId is Missing [KEY = {sheetKeyField} , SHEET_ID = {sheet?.Id}]");
                return result;
            }

            var slice = sheet.GetRow(sheetKeyField, key);
            result = ApplyData(source, value, slice);
            
            return result;
        }

        private IEnumerable<SyncField> LoadSyncFieldsData(Type sourceType, bool useAllFields)
        {
            var fields = sourceType.GetInstanceFields();

            var spreadsheetTargetAttribute = sourceType.
                GetCustomAttribute<SpreadsheetTargetAttribute>();
            
            var filedsAttributes = new List<SheetValueAttribute>();
            var keyFieldSheetName = GoogleSheetImporterConstants.KeyField;
            var keyFieldName = spreadsheetTargetAttribute != null ? 
                spreadsheetTargetAttribute.KeyField :
                GoogleSheetImporterConstants.KeyField;
            
            foreach (var field in fields) {
                var attributeInfo = field.
                    FieldType.
                    GetCustomAttribute<SheetValueAttribute>();
                filedsAttributes.Add(attributeInfo);
                if (attributeInfo != null && attributeInfo.isKey)
                    keyFieldName = attributeInfo.useFieldName ? field.Name : attributeInfo.dataField;
            }

            for (var i = 0; i < fields.Count; i++)
            {
                var fieldInfo       = fields[i];
                var customAttribute = filedsAttributes[i];
                if (customAttribute == null && !useAllFields)
                    continue;

                var fieldName = SheetData.FormatKey(fieldInfo.Name);
                var sheetField = customAttribute!=null && !customAttribute.useFieldName ? 
                    customAttribute.dataField : fieldName;

                var isKeyField = SheetData.IsEquals(keyFieldName, fieldName);
                var syncField = new SyncField(fieldInfo, sheetField,isKeyField);

                yield return syncField;
            }
        }

    }
}