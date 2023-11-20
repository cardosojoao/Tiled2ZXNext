namespace Tiled2ZXNext.Models
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRoot(Namespace = "", IsNullable = false)]
    public partial class TileSetXMl
    {
        private TilesetImage imageField;

        private TilesetTile tileField;

        private decimal versionField;

        private string tiledversionField;

        private string nameField;

        private int tilewidthField;

        private int tileheightField;

        private ushort tilecountField;

        private int columnsField;

        private TilesetTileProperty[] propertiesField;


        /// <remarks/>
        public TilesetImage image
        {
            get
            {
                return imageField;
            }
            set
            {
                imageField = value;
            }
        }



        /// <remarks/>
        public TilesetTile tile
        {
            get
            {
                return tileField;
            }
            set
            {
                tileField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public decimal version
        {
            get
            {
                return versionField;
            }
            set
            {
                versionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string tiledversion
        {
            get
            {
                return tiledversionField;
            }
            set
            {
                tiledversionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string name
        {
            get
            {
                return nameField;
            }
            set
            {
                nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public int tilewidth
        {
            get
            {
                return tilewidthField;
            }
            set
            {
                tilewidthField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public int tileheight
        {
            get
            {
                return tileheightField;
            }
            set
            {
                tileheightField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public ushort tilecount
        {
            get
            {
                return tilecountField;
            }
            set
            {
                tilecountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public int columns
        {
            get
            {
                return columnsField;
            }
            set
            {
                columnsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("property", IsNullable = false)]
        public TilesetTileProperty[] properties
        {
            get
            {
                return propertiesField;
            }
            set
            {
                propertiesField = value;
            }
        }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    public partial class TilesetImage
    {
        private string SourceField;

        private int WidthField;

        private int HeightField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string source
        {
            get
            {
                return SourceField;
            }
            set
            {
                SourceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public int width
        {
            get
            {
                return WidthField;
            }
            set
            {
                WidthField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public int Height
        {
            get
            {
                return HeightField;
            }
            set
            {
                HeightField = value;
            }
        }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    public partial class TilesetTile
    {
        private TilesetTileProperty[] propertiesField;

        private byte IdField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("property", IsNullable = false)]
        public TilesetTileProperty[] properties
        {
            get
            {
                return propertiesField;
            }
            set
            {
                propertiesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public byte id
        {
            get
            {
                return IdField;
            }
            set
            {
                IdField = value;
            }
        }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    public partial class TilesetTileProperty
    {

        private string nameField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string name
        {
            get
            {
                return nameField;
            }
            set
            {
                nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string value
        {
            get
            {
                return valueField;
            }
            set
            {
                valueField = value;
            }
        }
    }
}