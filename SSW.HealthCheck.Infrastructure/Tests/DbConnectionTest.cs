﻿namespace SSW.HealthCheck.Infrastructure.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Configuration;

    /// <summary>
    /// Database connection test
    /// </summary>
    public class DbConnectionTest : ITest
    {
        private readonly string name = Labels.DbTestTitle;
        private readonly string description = Labels.DbTestDescription;
        private readonly bool isDefault = true;
        private int order;
        private List<string> connectionStringNamesToBeExcluded = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DbConnectionTest" /> class.
        /// </summary>
        public DbConnectionTest(int order = 0)
        {
            this.Order = order;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbConnectionTest" /> class.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="connectionStringNamesToBeExcluded">The connection strings to be excluded.</param>
        public DbConnectionTest(int order = 0, List<string> connectionStringNamesToBeExcluded = null)
        {
            if (connectionStringNamesToBeExcluded != null)
            {
                this.connectionStringNamesToBeExcluded = connectionStringNamesToBeExcluded;
            }

            this.Order = order;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbConnectionTest" /> class.
        /// </summary>
        /// <param name="name">The test name.</param>
        /// <param name="order">The order in which test will appear in the list.</param>
        public DbConnectionTest(string name, int order = 0)
        {
            this.name = name;
            this.order = order;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbConnectionTest" /> class.
        /// </summary>
        /// <param name="name">The test name.</param>
        /// <param name="description">The test description.</param>
        /// <param name="order">The order in which test will appear in the list.</param>
        public DbConnectionTest(string name, string description, int order = 0)
        {
            this.name = name;
            this.description = description;
            this.order = order;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbConnectionTest" /> class.
        /// </summary>
        /// <param name="isDefault">Run test by default.</param>
        /// <param name="order">The order in which test will appear in the list.</param>
        public DbConnectionTest(bool isDefault, int order = 0)
        {
            this.isDefault = isDefault;
            this.order = order;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbConnectionTest" /> class.
        /// </summary>
        /// <param name="name">The test name.</param>
        /// <param name="description">The test description.</param>
        /// <param name="isDefault">
        /// Flag indicating if test will be run when page loads. 
        /// True - test will run everytime page is loaded, False - test will be triggered manually by user
        /// </param>
        /// <param name="order">The order in which test will appear in the list.</param>
        public DbConnectionTest(string name, string description, bool isDefault, int order = 0)
        {
            this.name = name;
            this.description = description;
            this.isDefault = isDefault;
            this.order = order;
        }

        /// <summary>
        /// Gets or sets the test category. Used for grouping of tests
        /// </summary>
        /// <value>The test category.</value>
        public TestCategory TestCategory { get; set; }

        /// <summary>
        /// Gets a value indicating whether test belongs to a category.
        /// </summary>
        /// <value></value>
        public bool HasCategory
        {
            get
            {
                return this.TestCategory != null;
            }
        }

        /// <summary>
        /// Gets the name for the test.
        /// </summary>
        /// <value></value>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Gets or sets the order in which test appears.
        /// </summary>
        /// <value>The order.</value>
        public int Order
        {
            get
            {
                return this.order;
            }

            set
            {
                this.order = value;
            }
        }

        /// <summary>
        /// Gets the description for test.
        /// </summary>
        /// <value></value>
        public string Description
        {
            get { return this.description; }
        }

        /// <summary>
        /// Gets a value that indicate if the test is to run by default.
        /// </summary>
        /// <value></value>
        public bool IsDefault
        {
            get { return this.isDefault; }
        }

        /// <summary>
        /// Gets or sets the include.
        /// </summary>
        /// <value>The include.</value>
        public string[] Include { get; set; }

        /// <summary>
        /// Gets or sets the exclude.
        /// </summary>
        /// <value>The exclude.</value>
        public string[] Exclude { get; set; }

        /// <summary>
        /// Gets the widget actions.
        /// </summary>
        /// <value>The test actions.</value>
        public IEnumerable<TestAction> TestActions
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Run the health check.
        /// </summary>
        /// <param name="ctx">Test context</param>
        public void Test(ITestContext ctx)
        {
            var settings = ConfigurationManager.ConnectionStrings.OfType<ConnectionStringSettings>().ToList();

            if (this.connectionStringNamesToBeExcluded != null && this.connectionStringNamesToBeExcluded.Any())
            {
                settings =
                    settings.Where(
                        s =>
                        !this.connectionStringNamesToBeExcluded.Any(
                            cs => s.Name.Equals(cs, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            var failedSettings = new System.Collections.Concurrent.ConcurrentBag<ConnectionStringSettings>();
            var settingsCount = settings.Count();
            var processedCount = 0;
            ctx.UpdateProgress(0, processedCount, settingsCount);
            Parallel.ForEach(
                settings.ToList(),
                setting =>
                {
                    try
                    {

                        var isEntityClient = setting.ProviderName == "System.Data.EntityClient";

                        if (!isEntityClient)
                        {
                            var factory = DbProviderFactories.GetFactory(setting.ProviderName);
                            var csBuilder = new DbConnectionStringBuilder();
                            csBuilder.ConnectionString = setting.ConnectionString;
                            csBuilder["Connection Timeout"] = 5;
                            var connectionString = csBuilder.ConnectionString;
                            using (var cnn = factory.CreateConnection())
                            {
                                if (cnn == null)
                                {
                                    ctx.WriteLine(
                                        EventType.Error,
                                        Errors.TestFailed,
                                        setting.Name,
                                        Errors.CannotCreateConnection);
                                    failedSettings.Add(setting);
                                }
                                else
                                {
                                    cnn.ConnectionString = connectionString;
                                    cnn.Open();
                                }
                            }
                        }
                        else
                        {
                            var csBuilder = new EntityConnectionStringBuilder(setting.ConnectionString);
                            csBuilder.Provider = "System.Data.SqlClient";
                            csBuilder.ProviderConnectionString = csBuilder.ProviderConnectionString + ";Connection Timeout = 5";
                            using (var entityConnection = new EntityConnection(csBuilder.ConnectionString))
                            {
                                entityConnection.Open();
                            }
                        }

                        ctx.WriteLine(EventType.Success, Labels.ConnectionSuccessful, setting.Name);
                    }
                    catch (Exception ex)
                    {
                        ctx.WriteLine(EventType.Error, Errors.TestFailed, setting.Name, ex.Message);
                        failedSettings.Add(setting);
                    }

                    processedCount++;
                    ctx.UpdateProgress(0, processedCount, settingsCount);
                });

            if (failedSettings.Count > 0)
            {
                var msg = string.Format(Errors.CannotOpenConnection, string.Join(", ", failedSettings.Select(x => x.Name).ToArray()));
                Assert.Fails(msg);
            }
        }
    }
}
