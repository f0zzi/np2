namespace server
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    public class DataContext : DbContext
    {
        // Your context has been configured to use a 'DataContext' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'server.DataContext' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'DataContext' 
        // connection string in the application configuration file.
        public DataContext()
            : base("name=DataContext")
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        public virtual DbSet<Index> Indexes { get; set; }
        public virtual DbSet<Street> Streets { get; set; }
    }

    public class Index
    {
        public Index()
        {
            Streets = new List<Street>();
        }
        public int Id { get; set; }
        public string PostIndex { get; set; }
        public virtual ICollection<Street> Streets{ get; set; }

    }
    public class Street
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int IndexId { get; set; }
        public virtual Index Index { get; set; }
    }
}