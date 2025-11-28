using UnityEngine;

public class MazeCell : MonoBehaviour
{
    //wall refrences
    [SerializeField]
    private GameObject LeftWall;

    [SerializeField]
    private GameObject RightWall;

    [SerializeField]
    private GameObject FrontWall;

    [SerializeField]
    private GameObject BackWall;

    //unvisited the the cell that is not yet visited
    [SerializeField]
    private GameObject Unvisited;

    //a bool that keep track on yo mama
    public bool IsVisited { get; private set; }

    public void Visit()//mark de cell as visited from unvisited
    {
        IsVisited = true;
        Unvisited.SetActive(false);


    }

    //clear the walls
    public void ClearLeftWall()
    {

        LeftWall.SetActive(false);

    }

    public void ClearRightWall()
    {
        RightWall.SetActive(false);
    }

    public void ClearFrontWall()
    {
        FrontWall.SetActive(false);

    }
    public void ClearBackWall()
    {
        BackWall.SetActive(false);
    }
}
