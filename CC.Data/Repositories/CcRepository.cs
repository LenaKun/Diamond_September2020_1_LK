using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Security;
using System.Security.Principal;

namespace CC.Data.Repositories
{
    public class CcRepository:IDisposable
    {
        private ccEntities _dataContext = null;
        public User CurrentUser { get; set; }
        public CcRepository()
        {
            _dataContext = new ccEntities();
            _dataContext.SavingChanges += new EventHandler(_dataContext_SavingChanges);

            CreateRepositories(_dataContext);
        }
        public CcRepository(User user):this()
        {
            this.CurrentUser = user;
            this._dataContext.ConetxtUser = user;
        }

		public bool GetLazyLoading()
		{
			return _dataContext.ContextOptions.LazyLoadingEnabled;
		}

		public void SetLazyLoading(bool _lazyLoading)
		{
			_dataContext.ContextOptions.LazyLoadingEnabled = _lazyLoading;
		}

        

        #region Repositories

        private void CreateRepositories(ccEntities dataContext)
        {
            Clients = new ClientsRepository(dataContext,this);
            HomeCareEntitledPeriods = new HomeCareEntitledPeriodsRepository(dataContext,this);
            FundStatuses = new Repository<FundStatus>(dataContext, this);
            Agencies = new AgenciesRepository(dataContext);
            AgencyGroups = new AgencyGroupsRepository(dataContext);
            ApprovalStatuses = new Repository<ApprovalStatus>(dataContext);
            Users = new Repository<User>(dataContext);
            FunctionalityScores= new FunctionalityScoresRepository(dataContext,this);
            FunctionalityLevels= new Repository<FunctionalityLevel>(dataContext);
            Countries = new Repository<Country>(dataContext);
            States = new Repository<State>(dataContext);
            NationalIdTypes= new Repository<NationalIdType>(dataContext);
            LeaveReasons= new Repository<LeaveReason>(dataContext);
            Histories = new Repository<History>(dataContext);
            Regions = new Repository<Region>(dataContext);
            DccSubsides = new Repository<DccSubside>(dataContext);
			CommunicationsPreferences = new Repository<CommunicationsPreference>(dataContext);
			CareReceivingOptions = new Repository<CareReceivingOption>(dataContext);
        }

        public IRepository<Client> Clients { get; set; }
        public IRepository<HomeCareEntitledPeriod> HomeCareEntitledPeriods { get; set; }
        public IRepository<FundStatus> FundStatuses { get; set; }
        public IRepository<Agency> Agencies { get; set; }
        public IRepository<AgencyGroup> AgencyGroups { get; set; }
        public IRepository<ApprovalStatus> ApprovalStatuses { get; set; }
        public IRepository<User> Users { get; set; }
        public IRepository<FunctionalityScore> FunctionalityScores { get; set; }
        public IRepository<FunctionalityLevel> FunctionalityLevels{ get; set; }
        public IRepository<Country> Countries { get; set; }
        public IRepository<State> States{ get; set; }
        public IRepository<NationalIdType> NationalIdTypes { get; set; }
        public IRepository<LeaveReason> LeaveReasons { get; set; }

        public IRepository<DccSubside> DccSubsides { get; set; }
        public IRepository<History> Histories { get; set; }
        public IRepository<Region> Regions { get; set; }
		public IRepository<CommunicationsPreference> CommunicationsPreferences { get; set; }
		public IRepository<CareReceivingOption> CareReceivingOptions { get; set; }

        #endregion

        #region SaveChanges

        public int SaveChanges(System.Data.Objects.SaveOptions options)
        {
            return _dataContext.SaveChanges(options);
        }
        public int SaveChanges()
        {
            return _dataContext.SaveChanges();
        }
        void _dataContext_SavingChanges(object sender, EventArgs e)
        {
               
        }

        #endregion

        public void Dispose()
        {
            if (_dataContext != null)
                _dataContext.Dispose();
        }
    }
}
