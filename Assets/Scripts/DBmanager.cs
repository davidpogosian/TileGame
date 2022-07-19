using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using Unity.Netcode;

public class DBmanager : NetworkBehaviour // made dbmanger static?
{
    string dbName = "URI=file:IDs.db";
    private void Start()
    {
        //CreateTServerRpc();
        //AddEntryServerRpc(string guid, int money)
        //EditEntryServerRpc(string guid, int newMoney)
        //DisplayerEntriesServerRpc();
        //PullEntryServerRpc(string guid);
        //DropTServerRPC();

        //string s = String.Format("It is now {0:d} at {0:t}", DateTime.Now);
        //Console.WriteLine(s);

    }
    [ServerRpc(RequireOwnership = false)]
    public void CreateTServerRpc()
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS guids (guid VARCHAR(40), money INT);";
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void AddEntryServerRpc(string guid, int money)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO guids (guid, money) VALUES ('" + guid + "', '" + money +"');" ;
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void EditEntryServerRpc(string guid, int newMoney)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE guids SET guid='" + guid + "', money=" + newMoney + " WHERE guid='" + guid + "';";
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void DisplayEntriesServerRpc()
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM guids;";

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Debug.Log("Guid: " + reader["guid"] + "\tMoney: " + reader["money"]);
                    }
                    reader.Close();
                }
            }
            connection.Close();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void DropTServerRPC()
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "DROP TABLE guids;";
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PullEntryServerRpc(string guid)
    {
        int money = 0;
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT money FROM guids WHERE guid='" + guid + "';";

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Debug.Log("Money: " + reader["money"]);
                        money = (int)reader["money"];
                    }
                    reader.Close();
                }
            }
            connection.Close();
        }
    }
}
