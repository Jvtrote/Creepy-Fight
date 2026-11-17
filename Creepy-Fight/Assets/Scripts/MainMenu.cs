using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Função chamada ao clicar no botão Jogar
    public void Jogar()
    {
        // Carrega a cena de Modos que acabamos de criar
        SceneManager.LoadScene("Modos");
    }

    // Função chamada ao clicar no botão Sair
    public void Sair()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }
}