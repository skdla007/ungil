using ArcGISControls.CommonData.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ArcGISControls.CommonData.Parsers
{
    public static class SplunkPostItParser
    {
        private const string TitleColumnName = "_IW_POSTIT_TITLE";

        private const string BodyColumnName = "_IW_POSTIT_BODY";

        private const string GoColumnName = "_IW_POSTIT_GO";

        private const string EditUpdateSplColumnName = "_IW_POSTIT_EDIT_UPDATE_SPL";

        private const string EditStatusSplColumnName = "_IW_POSTIT_EDIT_STATUS_SPL";

        private const string AppColumnName = "_IW_POSTIT_APP";

        public static SplunkPostItData Parse(DataRow dataRow)
        {
            if (dataRow == null
                || dataRow.Table == null
                || dataRow.Table.Columns == null)
                throw new ArgumentNullException();

            if (!dataRow.Table.Columns.Contains(TitleColumnName)) return null;

            var data = new SplunkPostItData();
            data.Title = dataRow.Field<string>(TitleColumnName);
            data.Body = dataRow.Table.Columns.Contains(BodyColumnName) ? dataRow.Field<string>(BodyColumnName) : null;
            data.Go = dataRow.Table.Columns.Contains(GoColumnName) ? dataRow.Field<string>(GoColumnName) : null;
            data.EditUpdateSpl = dataRow.Table.Columns.Contains(EditUpdateSplColumnName) ? dataRow.Field<string>(EditUpdateSplColumnName) : null;
            data.EditStatusSpl = dataRow.Table.Columns.Contains(EditStatusSplColumnName) ? dataRow.Field<string>(EditStatusSplColumnName) : null;
            data.App = dataRow.Table.Columns.Contains(AppColumnName) ? dataRow.Field<string>(AppColumnName) : null;
            return data;
        }
    }
}
