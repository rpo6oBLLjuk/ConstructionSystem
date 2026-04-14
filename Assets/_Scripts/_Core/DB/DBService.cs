using UnityEngine;
using Zenject;

public class DBService : MonoBehaviour
{
    [Inject] DBConfig config;


    public void OpenConnection()
    {

    }
}


/*  command.CommandText = @"
        INSERT INTO Players (Name, Level, Experience) 
        VALUES (@name, @level, @exp);
                    
        SELECT last_insert_rowid();
    ";
                
    command.Parameters.AddWithValue("@name", playerName);
    command.Parameters.AddWithValue("@level", startLevel);
    command.Parameters.AddWithValue("@exp", 0);
*/