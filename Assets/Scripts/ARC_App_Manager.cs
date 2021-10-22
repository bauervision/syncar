using UnityEngine;

public class ARC_App_Manager : MonoBehaviour
{
    public static ARC_App_Manager instance;
    public ARC_App current_app = null;

    private void Start()
    {
        instance = this;
    }

    public void Create_New_App(ARC_Library data)
    {
        current_app = new ARC_App(data);
    }
}