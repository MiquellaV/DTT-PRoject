using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MazeGenerator : MonoBehaviour
{
    //Input for width and height
    public TMP_InputField widthInput;
    public TMP_InputField heightInput;

    [SerializeField]
    //prefab used for the maze cells
    private MazeCell mazecellPrefab;

    [SerializeField]
    //width of the maze
    public int MazeWidth;

    [SerializeField]
    //height of the maze
    public int MazeHeight;

    private MazeCell[,] mazeGrid; //a 2D array saving all cells
    private bool isGenerating = false; //pervent double generation
    // ^ this bool basicly makes sure the player can't spam the button and break stuff


    //Use width and hieght from the input 
    public void UpdateMazeSizeFromInput()
    {
        int newWidth, newHeight;

        // trying to convert the text from the input fields into actual ints.
        //  so If the user types something weird (like letters), using TryParse avoids an error
        if (int.TryParse(widthInput.text, out newWidth))
        {
            
            MazeWidth = Mathf.Clamp(newWidth, 10, 250);//limit values
        }

        if (int.TryParse(heightInput.text, out newHeight))
        {
            MazeHeight = Mathf.Clamp(newHeight, 10, 250);
        }
    }

    //generate maze when called 
    public void GenerateNewMaze()
    {
        if (isGenerating) return;//to pervent genrating twice
        // if we’re already making a maze, just ignore the button pressing

        UpdateMazeSizeFromInput();
        StartCoroutine(GenerateMazeStartup());
        // I use a coroutine because maze creation happens step-by-step instead of instantly
    }

    public void StopAndClearMaze()
    {
        // stop anything that is still generating
        StopAllCoroutines();
        isGenerating = false;

        // destroy all the old cells so the new maze doesn't overlap
        if (mazeGrid != null)
        {
            foreach (MazeCell cell in mazeGrid)
            {
                if (cell != null)
                    Destroy(cell.gameObject);
            }
        }
    }

    private IEnumerator GenerateMazeStartup()
    {
        isGenerating = true;

        // Delete the old maze (same reason)
        if (mazeGrid != null)
        {
            foreach (MazeCell cell in mazeGrid)
            {
                if (cell != null)
                    Destroy(cell.gameObject);
            }
        }

        // Camera adjustment
        // I reposition the camera to always be centered on the maze no matter its size
        float centerX = MazeWidth / 2f - 0.5f;
        float centerZ = MazeHeight / 2f - 0.5f;
        Camera.main.transform.position = new Vector3(centerX, 20f, centerZ);
        Camera.main.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        Camera.main.orthographicSize = Mathf.Max(MazeWidth, MazeHeight) / 2f + 1f;
        // orthographicSize basically zooms the camera out so the whole maze fits in view

        // Create a new maze
        mazeGrid = new MazeCell[MazeWidth, MazeHeight];
        // making the actual grid so we can store every cell we spawn

        for (int x = 0; x < MazeWidth; x++)
        {
            for (int z = 0; z < MazeHeight; z++)
            {
                // spawning every cell at its correct position in the world
                mazeGrid[x, z] = Instantiate(mazecellPrefab, new Vector3(x, 0, z), Quaternion.identity);
            }
        }

        // Start maze generation
        // I start the recursive backtracking algorithm from the top-left cell (0,0)
        yield return GenerateMaze(null, mazeGrid[0, 0]);

        isGenerating = false;
    }

    IEnumerator GenerateMaze(MazeCell lastCell, MazeCell currentCell)
    {
        // Mark this cell as visited so we don't come back here again
        currentCell.Visit();

        // Remove the walls between this cell and the last one we came from
        ClearWalls(lastCell, currentCell);

        // delay so the maze builds visually over time 
        yield return new WaitForSeconds(0.05f);

        // find the next cell that has not been visited yet
        MazeCell nextCell = GetNextUnvisitedCell(currentCell);

        // as long as there's somewhere to go, we keep going deeper 
        //  but when there’s no more unvisited neighbors, the function returns 
        while (nextCell != null)
        {
            yield return StartCoroutine(GenerateMaze(currentCell, nextCell));
            nextCell = GetNextUnvisitedCell(currentCell);
        }
        
    }

    private MazeCell GetNextUnvisitedCell(MazeCell currentCell)
    {
        // get all neighbors that we haven't visited yet
        var unvisitedCells = GetUnvisitedCells(currentCell);

        // randomize pick so every maze is different
        return unvisitedCells.OrderBy(x => Random.Range(0f, 1f)).FirstOrDefault();
    }
    
    
    // using  IEnumerable is better here bc we dont need to make a whole list

    private IEnumerable<MazeCell> GetUnvisitedCells(MazeCell currentCell)
    {
        // this function  checks all 4 directions and  yields the ones we haven't visited yet

        int x = (int)currentCell.transform.position.x;
        int z = (int)currentCell.transform.position.z;

        // Right
        if (x + 1 < MazeWidth)
        {
            var cellRight = mazeGrid[x + 1, z];
            if (!cellRight.IsVisited)
                yield return cellRight;
        }

        // Left
        if (x - 1 >= 0)
        {
            var cellLeft = mazeGrid[x - 1, z];
            if (!cellLeft.IsVisited)
                yield return cellLeft;
        }

        // Forward
        if (z + 1 < MazeHeight)
        {
            var cellFront = mazeGrid[x, z + 1];
            if (!cellFront.IsVisited)
                yield return cellFront;
        }

        // Back
        if (z - 1 >= 0)
        {
            var cellBack = mazeGrid[x, z - 1];
            if (!cellBack.IsVisited)
                yield return cellBack;
        }
    }

    private void ClearWalls(MazeCell lastCell, MazeCell currentCell)
    {
        if (lastCell == null) return;

        // figure out what direction we moved and remove the correct walls on both cells
        Vector3 prevPos = lastCell.transform.position;
        Vector3 currPos = currentCell.transform.position;

        if (prevPos.x < currPos.x) // moved right
        {
            lastCell.ClearRightWall();
            currentCell.ClearLeftWall();
            return;
        }

        if (prevPos.x > currPos.x) // moved left
        {
            lastCell.ClearLeftWall();
            currentCell.ClearRightWall();
            return;
        }

        if (prevPos.z < currPos.z) // moved forward
        {
            lastCell.ClearFrontWall();
            currentCell.ClearBackWall();
            return;
        }

        if (prevPos.z > currPos.z) // moved backward
        {
            lastCell.ClearBackWall();
            currentCell.ClearFrontWall();
            return;
        }
    }
}
