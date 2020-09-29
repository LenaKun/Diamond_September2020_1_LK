using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Data.Objects;
using System.Linq;
using System.Text;

namespace CC.Data
{
    public class ValueChangedEventArgs<T> : EventArgs
    {
        public ValueChangedEventArgs() { }
        public ValueChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public T OldValue { get; set; }
        public T NewValue { get; set; }
    }

    public class ValueChangingEventArgs<T> : ValueChangedEventArgs<T>
    {
        public ValueChangingEventArgs() : base() { }

        private bool _cancel = false;
        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }

    }


    partial class ccEntities
    {

        public User ConetxtUser { get; set; }


        public ObjectContext ObjectContext
        {
            get
            {
                return (this as IObjectContextAdapter).ObjectContext;
            }
        }

        public ccEntities(User user)
            : this()
        {
            this.ConetxtUser = user;
        }
        void CcDataContext_ObjectMaterialized(object sender, System.Data.Objects.ObjectMaterializedEventArgs e)
        {
            var objectcontext = sender as ObjectContext;
        }


        void CcDataContext_SavingChanges(object sender, EventArgs e)
        {
            var objectcontext = sender as ObjectContext;
        }
        public override int SaveChanges()
        {

            //detect changes
            //this.DetectChanges();

            if (this.ConetxtUser != null)
            {
                
                foreach (var entry in ObjectContext.ObjectStateManager.GetObjectStateEntries(System.Data.EntityState.Modified | System.Data.EntityState.Added | System.Data.EntityState.Modified | System.Data.EntityState.Deleted))
                {

                    System.Reflection.PropertyInfo prop = null;
                    if (!entry.IsRelationship && entry.Entity != null)
                    {
                        prop = entry.Entity.GetType().GetProperty("UpdatedAt");
                        if (prop != null)
                        {
                            prop.SetValue(entry.Entity, DateTime.Now, null);
                        }

                        prop = entry.Entity.GetType().GetProperty("UpdatedById") ?? entry.Entity.GetType().GetProperty("UpdatedBy");
                        if (prop != null)
                        {
                            prop.SetValue(entry.Entity, this.ConetxtUser.Id, null);
                        }
                    }

                }
            }

            if (this.ConetxtUser != null)
            {
                //call PropertyChanging event trigger on objects that support it before sending field updates to the db
                var clientsUpdating =this.ObjectContext.ObjectStateManager.GetObjectStateEntries(System.Data.EntityState.Modified);
                var permissions = CC.Data.Services.PermissionsFactory.GetPermissionsFor(this.ConetxtUser);
                foreach (var entry in clientsUpdating)
                {
                    foreach (var propertyName in entry.GetModifiedProperties())
                    {
                        var propChangingNotifier = entry.Entity as INotifyPropertySaving;
                        if (propChangingNotifier != null)
                        {

                            ValidationContext validationContext = new ValidationContext(entry, null, new Dictionary<object, object>() { { typeof(User), this.ConetxtUser } });

                            try
                            {
                                //notifiy an object of a property update will be set to a database
                                propChangingNotifier.OnPropertySaving(propertyName, permissions);

                            }
                            catch (PropertyChangeDeniedException)
                            {
                                //the property change is invalid
                            }
                        }
                    }

                }
            }


            return base.SaveChanges();


        }


        public static void InitSampleData()
        {
            Random rnd = new Random();
            //insert enums

            CC.Data.Init.Ser.InitCountries();

            CC.Data.Init.Ser.InitEnums();

        }

        public static IEnumerable<Client> SampleCusomers(int count)
        {
            Random rnd = new Random();

            using (var db = new ccEntities())
            {
                var countries = db.Countries.ToList();
                var agencies = db.Agencies.ToList();
                var users = db.Users.ToList();
                return Enumerable.Range(0, count).Select(c =>
                {
                    int i = rnd.Next(int.MaxValue);
                    return new Client()
                    {
                        FirstName = string.Format("fn{0}", i),
                        LastName = string.Format("ln{0}", i),
                        JoinDate = DateTime.Now,
                        CountryId = countries.Skip(i % countries.Count).FirstOrDefault().Id,
                        AgencyId = agencies.Skip(i % agencies.Count).FirstOrDefault().Id,
                        City = string.Format("fn{0}", i),
                        Address = string.Format("fn{0}", i),
                        UpdatedById = users.Skip(i % users.Count).FirstOrDefault().Id,
                        UpdatedAt = DateTime.Now
                    };
                });
            }
        }
    }

    public static class exxt
    {
        public static IEnumerable<IEnumerable<T>> ToChunksOf<T>(this IEnumerable<T> input, int count)
        {
            IEnumerable<T> take = null;
            try
            {
                take = input.Take(count);
            }
            catch (Exception)
            {
                yield break;
            }
            yield return take;
        }

    }
}
