using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{


    public void LoadScene(string scene){

        SceneManager.LoadScene(scene);
    }
  
}
