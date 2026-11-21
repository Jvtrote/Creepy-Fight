using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
    // Conecte essa função nos botões dos rostos, passando 0, 1, 2 ou 3
    public void SelectCharacter(int characterIndex)
    {
        GameData.player1CharacterIndex = characterIndex;

        // Por padrão no 1P vs CPU, escolhe um oponente (ex: o próximo da lista)
        GameData.player2CharacterIndex = (characterIndex + 1) % 4;

        // Carrega a sua arena principal
        SceneManager.LoadScene("Arena2");
    }
}