using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using System.IO;
using System.Data.SqlClient;
using Microsoft.Synchronization.Data.SqlServer;

namespace SyncFramework
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string sScope = "MyScopes";
        string sServerConnection = "Data Source=.; Initial Catalog=SyncDBServer; Integrated Security=True";
        string sClientConnection = "Data Source=.; Initial Catalog=ClientDB; Integrated Security=True";
        public MainWindow()
        {
            InitializeComponent();
        }
       
        private void Button_Click(object sender, RoutedEventArgs e)
        {
          Deprovision2();
            ProvisionServer();

            ProvisionClient();
            Sync();
            #region Create a Scope
            //SqlConnection serverConn = new SqlConnection("Data Source=.; Initial Catalog=SyncDBServer; Integrated Security=True");

            //// define a new scope named MySyncScope
            //DbSyncScopeDescription scopeDesc = new DbSyncScopeDescription("MySyncScope");

            //// get the description of the CUSTOMER & PRODUCT table from SERVER database
            //DbSyncTableDescription cusTableDesc = SqlSyncDescriptionBuilder.GetDescriptionForTable("CUSTOMER", serverConn);
            //DbSyncTableDescription prodTableDesc = SqlSyncDescriptionBuilder.GetDescriptionForTable("PRODUCT", serverConn);

            //// add the table description to the sync scope definition
            //scopeDesc.Tables.Add(cusTableDesc);
            //scopeDesc.Tables.Add(prodTableDesc);

            //// create a server scope provisioning object based on the MySyncScope
            //SqlSyncScopeProvisioning serverProvision = new SqlSyncScopeProvisioning(serverConn, scopeDesc);

            //// skipping the creation of table since table already exists on server
            //serverProvision.SetCreateTableDefault(DbSyncCreationOption.Skip);

            //// start the provisioning process
            //serverProvision.Apply();

            //Console.WriteLine("Server Successfully Provisioned.");
            //Console.ReadLine();
            #endregion
        }

       
        public  void ProvisionServer()

        {

            SqlConnection serverConn = new SqlConnection(sServerConnection);



            DbSyncScopeDescription scopeDesc = new DbSyncScopeDescription(sScope);



            DbSyncTableDescription tableDesc = SqlSyncDescriptionBuilder.GetDescriptionForTable("CUSTOMER", serverConn);

            scopeDesc.Tables.Add(tableDesc);


            DbSyncTableDescription productDescription2 =
                                               SqlSyncDescriptionBuilder.GetDescriptionForTable("MOB",
                                                                                                    serverConn);
            scopeDesc.Tables.Add(productDescription2);



            SqlSyncScopeProvisioning serverProvision = new SqlSyncScopeProvisioning(serverConn, scopeDesc);

            serverProvision.SetCreateTableDefault(DbSyncCreationOption.Skip);

            if (!serverProvision.ScopeExists(sScope))

                serverProvision.Apply();

        }


        public  void Deprovision2()
        {
            try
            {
                SqlConnection serverConn = new SqlConnection(sServerConnection);

                // Connection to SQL client
                SqlConnection clientConn = new SqlConnection(sClientConnection);

                // Create Scope Deprovisioning for Sql Server and SQL client.
                SqlSyncScopeDeprovisioning serverSqlDepro = new SqlSyncScopeDeprovisioning(serverConn);
                SqlSyncScopeDeprovisioning clientSqlDepro = new SqlSyncScopeDeprovisioning(clientConn);

                // Remove the scope from SQL Server remove all synchronization objects.
                serverSqlDepro.DeprovisionScope(sScope);
                serverSqlDepro.DeprovisionStore();

                // Remove the scope from SQL client and remove all synchronization objects.
                clientSqlDepro.DeprovisionScope(sScope);
                clientSqlDepro.DeprovisionStore();

                // Shut down database connections.
                serverConn.Close();
                serverConn.Dispose();
                clientConn.Close();
                clientConn.Dispose();
            }
            catch (SqlException ex)
            {

            }
            catch (Microsoft.Synchronization.Data.DbVersionException ex)
            {

            }
            catch (Exception ex)
            {

            }
        }
            public  void ProvisionClient()

        {

            SqlConnection serverConn = new SqlConnection(sServerConnection);

            SqlConnection clientConn = new SqlConnection(sClientConnection);



            DbSyncScopeDescription scopeDesc = SqlSyncDescriptionBuilder.GetDescriptionForScope(sScope, serverConn);

            SqlSyncScopeProvisioning clientProvision = new SqlSyncScopeProvisioning(clientConn, scopeDesc);


            if (!clientProvision.ScopeExists(sScope))
                clientProvision.Apply();

        }
        private  void Sync()

        {

            try
            {
                // Connection to  SQL Server
                SqlConnection serverConn = new SqlConnection(sServerConnection);

                // Connection to SQL client
                SqlConnection clientConn = new SqlConnection(sClientConnection);

                // Perform Synchronization between SQL Server and the SQL client.
                SyncOrchestrator syncOrchestrator = new SyncOrchestrator();

                // Create provider for SQL Server
                SqlSyncProvider serverProvider = new SqlSyncProvider(sScope, serverConn);

                // Set the command timeout and maximum transaction size for the SQL Azure provider.
                SqlSyncProvider clientProvider = new SqlSyncProvider(sScope, clientConn);

                // Set Local provider of SyncOrchestrator to the server provider
                syncOrchestrator.LocalProvider = serverProvider;

                // Set Remote provider of SyncOrchestrator to the client provider
                syncOrchestrator.RemoteProvider = clientProvider;

                // Set the direction of SyncOrchestrator session to Upload and Download
                syncOrchestrator.Direction = SyncDirectionOrder.UploadAndDownload;

                // Create SyncOperations Statistics Object
                SyncOperationStatistics syncStats = syncOrchestrator.Synchronize();

                // Display the Statistics
                string s = null;
                s += "Start Time: " + syncStats.SyncStartTime + "---";
                s += "Total Changes Uploaded: " + syncStats.UploadChangesTotal + "---";
                s += "Total Changes Downloaded: " + syncStats.DownloadChangesTotal + "---";
                s += "Complete Time: " + syncStats.SyncEndTime;

                // Shut down database connections.
                serverConn.Close();
                serverConn.Dispose();
                clientConn.Close();
                clientConn.Dispose();
                MessageBox.Show(s);
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }



        }



        static void Program_ApplyChangeFailed(object sender, DbApplyChangeFailedEventArgs e)

        {

            MessageBox.Show(e.Conflict.Type.ToString());

            MessageBox.Show(e.Error.ToString());

        }

    }
}

