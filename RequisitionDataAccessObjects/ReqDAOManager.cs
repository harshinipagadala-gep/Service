using Gep.Cumulus.CSM.BaseDataAccessObjects;
using Gep.Cumulus.CSM.Config;
using Gep.Cumulus.CSM.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GEP.Cumulus.P2P.Req.DataAccessObjects
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// 2.0 DAO Manager.
    /// </summary>
    public static class ReqDAOManager
    {

        #region Protected methods
        /// <summary>
        /// Initializes DAO list.
        /// </summary>
        /// <returns>DAO dictionary.</returns>
        private static IDictionary<Type, object> InitializeDaoList(bool includeNewDAO = false)
        {
            IDictionary<Type, object> daoList = new Dictionary<Type, object>();
            daoList.Add(typeof(IRequisitionDAO), new SQLRequisitionDAO());
            daoList.Add(typeof(IRequisitionInterfaceDAO), new RequisitionInterfaceDAO());
            daoList.Add(typeof(IRequisitionCommonDAO), new RequisitionCommonDAO());

            if (includeNewDAO)
            {
                daoList.Add(typeof(INewRequisitionDAO), new NewRequisitionDAO());
            }

            return daoList;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Function which return the DAO Proxy object based on the input parameter "T".
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static T GetDAO<T>(UserExecutionContext context, GepConfig config) where T : IBaseDAO
        {
            IDictionary<Type, object> DaoList = InitializeDaoList();
            var dao = (T)DaoList[typeof(T)];
            dao.UserContext = context;
            dao.GepConfiguration = config;
            return dao;
        }
        public static T GetDAO<T>(UserExecutionContext context, GepConfig config, bool includeNewDAO) where T : IBaseDAO
        {
            IDictionary<Type, object> DaoList = InitializeDaoList(includeNewDAO);
            var dao = (T)DaoList[typeof(T)];
            dao.UserContext = context;
            dao.GepConfiguration = config;
            return dao;
        }


        /// <summary>
        /// Gets nullable decimal value.
        /// </summary>
        /// <param name="reader">Reader.</param>
        /// <param name="ColumnName">Column.</param>
        /// <returns>Nullable decimal.</returns>
        public static Nullable<decimal> GetNullableDecimalValue(System.Data.SqlClient.SqlDataReader reader, string ColumnName)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            decimal? returnValue = null;
            int columnIndex = reader.GetOrdinal(ColumnName);
            if (!Convert.IsDBNull(reader.GetValue(columnIndex)))
                returnValue = reader.GetDecimal(columnIndex);
            return returnValue;
        }

        public static Nullable<bool> GetNullableBooleanValue(System.Data.SqlClient.SqlDataReader reader, string ColumnName)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            bool? returnValue = null;
            int columnIndex = reader.GetOrdinal(ColumnName);
            if (!Convert.IsDBNull(reader.GetValue(columnIndex)))
                returnValue = reader.GetBoolean(columnIndex);
            return returnValue;
        }
        #endregion
    }
}
