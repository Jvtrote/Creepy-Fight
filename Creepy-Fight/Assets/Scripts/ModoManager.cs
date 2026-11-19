using UnityEngine;
using UnityEngine.SceneManagement;

public class ModoManager : MonoBehaviour
{
    // Vai para a cena de seleção de personagem (1P vs CPU)
    public void Selecionar1PvsCPU()
    {
        SceneManager.LoadScene("SelecaoPersonagem");
    }

    // Vai para a cena de configuração multiplayer (2P vs CPU)
    public void Selecionar2PvsCPU()
    {
        SceneManager.LoadScene("MultiplayerConfig");
    }

    // Vai para a cena de configuração multiplayer (Player vs Player)
    public void SelecionarPvsP()
    {
        SceneManager.LoadScene("MultiplayerConfig");
    }

    // Botão opcional para voltar ao Menu Principal
    public void VoltarMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}