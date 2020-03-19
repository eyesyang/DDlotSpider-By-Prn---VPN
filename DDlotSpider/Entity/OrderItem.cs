using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using CentaLine.Common;
using DDlotSpider.Service;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DDlotSpider.Entity
{
    [Serializable]
    public class OrderItem
    {
        public bool NextSearch { get; set; } = true;
        public string Machine { get; set; }

        public int Seq { get; set; }

       
        public string LotDescription { get; set; }

        public bool Ex { get; set; }

        public string LotType { get; set; }

        public string LotNo { get; set; }

        public string LotSection1 { get; set; }

        public string LotSection2 { get; set; }
        public string LotSection3 { get; set; }

        public string LotSection4 { get; set; }
        public string LotSection5 { get; set; }


        public string HouseNo { get; set; } = string.Empty;

        public string StreetName { get; set; } = string.Empty;

        public object FieldName { get; set; } = string.Empty;
        public object Keyword { get; set; } = string.Empty;
        public object Block { get; set; } = string.Empty;
        public object Lot { get; set; } = string.Empty;

        public string LotName { get; set; } = string.Empty;
        public object BlockDesc { get; set; } = string.Empty;
        public object Floor { get; set; } = string.Empty;
        public string Flat { get; set; }
        public string Prn { get; set; }
        public string AddressLot { get; set; }
        public string IsClosed { get; set; }
        public ProgressStatus Status { get; set; }

        public DateTime CreateDate { get; set; }

        public int LotCount { get; set; }
        public int LotIdx { get; set; }
        public int BlockCount { get; set; }
        public int BlockIdx { get; set; }
        public int FloorCount { get; set; }
        public int FloorIdx { get; set; }
        public OrderItem() { }
        public OrderItem(DataRow row)
        {
            this.Seq = ConvertUtility.ToInt(row["Seq"]);

            this.LotDescription = ConvertUtility.Trim(row["LotDescription"]);

            this.LotType = ConvertUtility.Trim(row["LotType"]);

            this.LotNo = ConvertUtility.Trim(row["LotNo"]);

            this.LotSection1 = ConvertUtility.Trim(row["LotSection1"]);
            this.LotSection2 = ConvertUtility.Trim(row["LotSection2"]);
            this.LotSection3 = ConvertUtility.Trim(row["LotSection3"]);
            this.LotSection4 = ConvertUtility.Trim(row["LotSection4"]);
            this.LotSection5 = ConvertUtility.Trim(row["LotSection5"]);

            this.Status = ProgressStatus.Search;
        }

        public void UpdateStatus()
        {
            DbService.UpdateOrder(this);
        }

        public void SetStatus()
        {
            if (File.Exists(AppSettings.StatusFileName))
            {
                File.Delete(AppSettings.StatusFileName);
            }

            using (var fileStream = new FileStream(AppSettings.StatusFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                var bf = new BinaryFormatter();
                bf.Serialize(fileStream, this);
            }
        }

        public void Save2Db()
        {

            this.AddressLot = ConvertUtility.Trim(AddressLot);

            this.Block = ConvertUtility.Trim(Block);

            this.BlockDesc = ConvertUtility.Trim(BlockDesc);

            this.FieldName = ConvertUtility.Trim(FieldName);

            this.Flat = ConvertUtility.Trim(Flat);

            this.Floor = ConvertUtility.Trim(Floor);

            this.HouseNo = ConvertUtility.Trim(HouseNo);

            this.IsClosed = ConvertUtility.Trim(IsClosed);

            this.Lot = ConvertUtility.Trim(Lot);

            this.LotName = ConvertUtility.Trim(LotName);

            this.LotDescription = ConvertUtility.Trim(LotDescription);

            this.Prn = ConvertUtility.Trim(Prn);

            this.StreetName = ConvertUtility.Trim(StreetName);

            this.CreateDate = DateTime.Now;

            DbService.Save2Db(this);

            //Init();
        }

        public void Init()
        {
            this.AddressLot = string.Empty;

            this.Block = string.Empty;

            this.BlockDesc = string.Empty;

            this.FieldName = string.Empty;

            this.Flat = string.Empty;

            this.Floor = string.Empty;

            this.HouseNo = string.Empty;

            this.IsClosed = string.Empty;

            this.Lot = string.Empty;

            this.LotName = string.Empty;

            this.Prn = string.Empty;

            this.StreetName = string.Empty;
        }

    }
}
