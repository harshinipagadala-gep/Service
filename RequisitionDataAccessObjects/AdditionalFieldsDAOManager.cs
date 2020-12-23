using Gep.Cumulus.CSM.BaseDataAccessObjects;
using Gep.Cumulus.CSM.Config;
using Gep.Cumulus.CSM.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GEP.Cumulus.P2P.Req.DataAccessObjects
{
    [ExcludeFromCodeCoverage]
    public class AdditionalFieldsDAOManager
    {

        #region Protected methods
        /// <summary>
        /// Initializes DAO list.
        /// </summary>
        /// <returns>DAO dictionary.</returns>
        private static IDictionary<Type, object> InitializeDaoList()
        {
            IDictionary<Type, object> daoList = new Dictionary<Type, object>();
            daoList.Add(typeof(IAdditionalFieldsDAO), new AdditionalFieldsDAO());
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

        #endregion

    }
}
