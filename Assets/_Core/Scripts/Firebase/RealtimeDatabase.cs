using Firebase.Database;
using System.Threading.Tasks;
using UnityEngine;
public class RealtimeDatabase : MonoBehaviour
{
    DatabaseReference dbRef;
    private string uuid;

    private void Awake()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        uuid = SystemInfo.deviceUniqueIdentifier;
    }
}