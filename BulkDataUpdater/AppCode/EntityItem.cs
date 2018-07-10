﻿namespace Cinteros.XTB.BulkDataUpdater.AppCode
{
    using Cinteros.Xrm.XmlEditorUtils;
    using Microsoft.Xrm.Sdk.Metadata;

    public class EntityItem : IComboBoxItem
    {
        private EntityMetadata meta = null;

        public EntityItem(EntityMetadata Entity)
        {
            meta = Entity;
        }

        public override string ToString()
        {
            return BulkDataUpdater.GetEntityDisplayName(meta);
        }

        public string GetValue()
        {
            return meta.LogicalName;
        }
    }
}
