using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
namespace CC.Extensions.Data
{
    public static class ListToDataTable
    {
        
        public static DataTable GetTable<T>(IEnumerable<T> varlist,List<string> columns)
        {
            DataTable dtReturn = new DataTable();


         


            if (varlist == null) return dtReturn;

              foreach (string s in columns)
                    {
                    

                        dtReturn.Columns.Add(new DataColumn(s));
                    }
             


            foreach (T rec in varlist)
            {
              
                  

                DataRow dr = dtReturn.NewRow();
                List<string> rowData = rec as List<string>;
                int i = 0;
                foreach (string cellData in rowData)
                {
                    dr[i] = cellData;
                    i++;
                }
              

                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }

    
    }


}
