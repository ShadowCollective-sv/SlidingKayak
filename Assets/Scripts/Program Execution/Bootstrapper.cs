using UnityEngine;
using UnityEngine.SceneManagement;

namespace Program_Execution
{
public class Bootstrapper : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        InitializeManagers();
        SceneManager.LoadScene("MainMenu");

    }

    

    private void InitializeManagers()
    {
        //throw new System.NotImplementedException();
    }
    
    
}

}