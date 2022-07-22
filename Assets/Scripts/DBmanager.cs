using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using Unity.Netcode;
using System.IO;

public class DBmanager : NetworkBehaviour // made dbmanger static?
{
    string dbName = "URI=file:IDs.db";
    private void Start()
    {
        if (!NetworkManager.Singleton.IsServer) { return; }
        string path = Directory.GetCurrentDirectory();
        //System.IO.File.Delete("IDs.db");
        if (!System.IO.File.Exists("IDs.db")) // doesnt work on the first try
        {
            RunSql("CREATE TABLE Players (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL unique, guid VARCHAR(40));");
            RunSql("CREATE TABLE Upgrades (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL unique, upgrade VARCHAR(40));");
            RunSql("CREATE TABLE PandU (player_id INTEGER , upgrade_id INTEGER);");

            RunSql("INSERT INTO Upgrades (upgrade) VALUES ('Vitality');");
            RunSql("INSERT INTO Upgrades (upgrade) VALUES ('Strength');");
            RunSql("INSERT INTO Upgrades (upgrade) VALUES ('Wall');");
        }
        //CreateTServerRpc();
        //AddEntryServerRpc(string guid, int money)
        //EditEntryServerRpc(string guid, int newMoney)
        //DisplayerEntriesServerRpc();
        //PullEntryServerRpc(string guid);
        //DropTServerRPC();
        

    }
    //[ServerRpc(RequireOwnership = false)]
    public void RunSql(string text)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = text;
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void RunSqlServerRpc(string text)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = text;
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }
    public int Pullid(string text)
    {
        int id = 0;
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = text;

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        //Debug.Log(reader["id"]);                    // this is bad
                        string s = reader["id"].ToString();
                        id = int.Parse(s);
                    }
                    reader.Close();
                }
            }
            connection.Close();
        }
        return id;
    }
    [ServerRpc(RequireOwnership = false)]
    public void PullPlayerUpgradesServerRpc(string guid, ulong clientID)
    {
        int player_id = Pullid("SELECT id FROM Players WHERE guid = '" + guid + "';");
        ClientRpcParams crp1 = new();
        crp1.Send.TargetClientIds = new ulong[] { clientID };

        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT upgrade_id FROM PandU WHERE player_id = '" + player_id + "';";

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        //Debug.Log(reader["upgrade_id"]);                    // this is bad
                        string s = reader["upgrade_id"].ToString();
                        int upgrade = int.Parse(s);
                        SendUpgradesClientRpc(upgrade, crp1);
                    }
                    reader.Close();
                }
            }
            connection.Close();
        }
    }
    [ClientRpc]
    public void SendUpgradesClientRpc(int upgrade, ClientRpcParams clientRpcParams)
    {
        switch (upgrade)
        {
            case 1: 
                NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerC>().upgrade1 = true;
                break;
            case 2:
                NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerC>().upgrade2 = true;
                break;
        }
    }
    //public void CreateT(string tableName)
    //{
    //    using (var connection = new SqliteConnection(dbName))
    //    {
    //        connection.Open();

    //        using (var command = connection.CreateCommand())
    //        {
    //            string sqlCommand = string.Format("CREATE TABLE IF NOT EXISTS {0} (guid VARCHAR(40), money INT);", tableName);
    //            command.CommandText = sqlCommand;
    //            command.ExecuteNonQuery();
    //        }
    //        connection.Close();
    //    }
    //}
    //[ServerRpc(RequireOwnership = false)]
    //public void AddEntryServerRpc(string guid, int money)
    //{
    //    using (var connection = new SqliteConnection(dbName))
    //    {
    //        connection.Open();

    //        using (var command = connection.CreateCommand())
    //        {
    //            command.CommandText = "INSERT INTO guids (guid, money) VALUES ('" + guid + "', '" + money +"');" ;
    //            command.ExecuteNonQuery();
    //        }
    //        connection.Close();
    //    }
    //}
    //[ServerRpc(RequireOwnership = false)]
    //public void EditEntryServerRpc(string guid, int newMoney)
    //{
    //    using (var connection = new SqliteConnection(dbName))
    //    {
    //        connection.Open();

    //        using (var command = connection.CreateCommand())
    //        {
    //            command.CommandText = "UPDATE guids SET guid='" + guid + "', money=" + newMoney + " WHERE guid='" + guid + "';";
    //            command.ExecuteNonQuery();
    //        }
    //        connection.Close();
    //    }
    //}
    //[ServerRpc(RequireOwnership = false)]
    //public void DisplayEntriesServerRpc()
    //{
    //    using (var connection = new SqliteConnection(dbName))
    //    {
    //        connection.Open();

    //        using (var command = connection.CreateCommand())
    //        {
    //            command.CommandText = "SELECT * FROM guids;";

    //            using (IDataReader reader = command.ExecuteReader())
    //            {
    //                while (reader.Read())
    //                {
    //                    Debug.Log("Guid: " + reader["guid"] + "\tMoney: " + reader["money"]);
    //                }
    //                reader.Close();
    //            }
    //        }
    //        connection.Close();
    //    }
    //}
    //[ServerRpc(RequireOwnership = false)]
    //public void DropTServerRPC()
    //{
    //    using (var connection = new SqliteConnection(dbName))
    //    {
    //        connection.Open();

    //        using (var command = connection.CreateCommand())
    //        {
    //            command.CommandText = "DROP TABLE guids;";
    //            command.ExecuteNonQuery();
    //        }
    //        connection.Close();
    //    }
    //}

    //[ServerRpc(RequireOwnership = false)]
    //public void PullEntryServerRpc(string guid)
    //{
    //    int money = 0;
    //    using (var connection = new SqliteConnection(dbName))
    //    {
    //        connection.Open();

    //        using (var command = connection.CreateCommand())
    //        {
    //            command.CommandText = "SELECT money FROM guids WHERE guid='" + guid + "';";

    //            using (IDataReader reader = command.ExecuteReader())
    //            {
    //                while (reader.Read())
    //                {
    //                    Debug.Log("Money: " + reader["money"]);
    //                    money = (int)reader["money"];
    //                }
    //                reader.Close();
    //            }
    //        }
    //        connection.Close();
    //    }
    //}
}
