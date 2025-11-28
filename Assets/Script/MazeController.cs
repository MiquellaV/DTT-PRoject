using UnityEngine;
using UnityEngine.InputSystem;

public class MazeInputController : MonoBehaviour
{
    public MazeGenerator generator; 

    private MazeControlls controls;

    private void Awake()
    {
        controls = new MazeControlls();

        // Keybinds for Start (Enter) and Restart (R)
        controls.Controls.Start.performed += ctx => OnStartMaze();
        controls.Controls.Restart.performed += ctx => OnRestartMaze();
    }

    private void OnEnable() => controls.Controls.Enable();
    private void OnDisable() => controls.Controls.Disable();

    private void OnStartMaze()
    {
        generator.GenerateNewMaze(); 
    }

    private void OnRestartMaze()
    {
        generator.StopAndClearMaze(); 
    }
}
