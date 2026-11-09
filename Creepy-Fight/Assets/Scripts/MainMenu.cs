using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Função chamada ao clicar no botão Jogar
    public void Jogar()
    {
        // Carrega a cena da Arena (certifique-se de que o nome da cena de luta é exatamente "Arena")
        SceneManager.LoadScene("Arena2");
    }

    // Função chamada ao clicar no botão Sair
    public void Sair()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit(); // Fecha o jogo (funciona na versão compilada)
    }
}