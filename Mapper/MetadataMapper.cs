using Entity = Tiled2dot8.Entities;
using Model = Tiled2dot8.Models;


namespace Tiled2dot8.Mapper
{
    public class MetadataMapper
    {
        public static Entities.Metadata Map(Model.MetaData metadata)
        {
            Entity.Metadata meta = new();

            meta.Enabled = metadata.Enabled;
            meta.Name = metadata.Name;
            meta.Modified = metadata.Modified;
            meta.Path = metadata.Path;
            meta.Columns = metadata.Columns;
            meta.Height = metadata.Height;
            meta.Rows = metadata.Rows;
            meta.Columns = metadata.Columns;
            meta.Height = metadata.Height;
            return meta;
        }
    }
}
