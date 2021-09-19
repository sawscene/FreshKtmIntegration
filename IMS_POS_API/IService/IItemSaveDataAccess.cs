using ClientApi.Models;
using IMS_POS_API.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMS_POS_API.IService
{
    public interface IItemSaveDataAccess
    {
        FunctionResponse Save(Product product);
        FunctionResponse SaveItem(Product product, List<BarCode> BCodeDetail, List<MULTIALTUNIT> AltUnit);
        FunctionResponse GetExistingProductMCode(IList<string> MCodeList, string Type = "A");
    }
}
