using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneGenerator : MonoBehaviour {
    public float maxVariance = 2f;
    public int count = 16;
    public GameObject InitialColumn;
    public Transform ball;
    public GameObject targetPrefab;
    public int targetFrequency = 20;

    private List<GameObject> m_ColumnList;
    private int m_OldestColumnIndex;
    private int m_NewestColumnIndex;
    private float m_LastAppliedBallZ;
    private float m_Hue;
    private int targetCounter;
    private float m_LaneSize;

	// Use this for initialization
	void Start () {
        // Record lane width, which allows accurate positioning
        m_LaneSize = InitialColumn.transform.localScale.z;
        // Creat list of columns
        m_ColumnList = new List<GameObject>();
        for (int i = 0; i < count; i++)
        {
            // First one exists, just add it and set it's color
            if (i == 0)
            {
                m_ColumnList.Add(InitialColumn);
                SetColor(0);
            }
            // Others are created & added. Color is set by PlaceColumn
            else
            {
                GameObject newColumn = Instantiate(InitialColumn);
                newColumn.transform.parent = InitialColumn.transform.parent;
                m_ColumnList.Add(newColumn);
                PlaceColumn(m_ColumnList.Count - 1, m_ColumnList[m_ColumnList.Count - 2].transform.position, 1);
            }
        }
        // Start hue value, increment for each new row
        m_Hue = 0;

        // Reference indexes for the newest and oldest columns. We move the oldest, and put it next to the newest.
        m_OldestColumnIndex = 0;
        m_NewestColumnIndex = m_ColumnList.Count - 1;
	}
	
	// Update is called once per frame
	void Update () {
        // Check if the ball position has move a full column unit. If so, move the oldest to the next available position. Happens off screen.
		if (ball.position.z > m_LastAppliedBallZ + m_LaneSize)
        {
            PlaceColumn(m_OldestColumnIndex, m_ColumnList[m_NewestColumnIndex].transform.position, maxVariance);
            
            // Update newest and oldest rows.
            m_NewestColumnIndex = m_OldestColumnIndex;
            m_OldestColumnIndex = (m_OldestColumnIndex + 1) % m_ColumnList.Count;
            m_LastAppliedBallZ += m_LaneSize;
        }
	}

    private void PlaceColumn(int columnIndex, Vector3 prevColumnPos, float maximumVariance )
    {
        // Get the desired position, one column width downstream from the latest column
        Vector3 movePos = new Vector3(prevColumnPos.x, Random.Range(prevColumnPos.y - maximumVariance, prevColumnPos.y + maximumVariance), prevColumnPos.z + m_LaneSize);
        // Move to that new position
        m_ColumnList[columnIndex].transform.position = movePos;
        // Set new color (with increment hue)
        SetColor(columnIndex);
        // Keep track of how many rows are generated since the last target.
        targetCounter++;
        // If the row contains a target, remove it.
        if (m_ColumnList[columnIndex].transform.Find("Goal"))
        {
            Destroy(m_ColumnList[columnIndex].transform.Find("Goal").gameObject);
        }
        // If it's time to add a target, do so.
        if (targetCounter >= targetFrequency)
        {
            GameObject target = Instantiate(targetPrefab);
            target.transform.parent = m_ColumnList[columnIndex].transform;
            // Name it specifically so we can detect it for removal later.
            target.name = "Goal";
            // Randomize height
            target.transform.localPosition = new Vector3(0, Random.Range(-5f, 5f), 0);
            // Reset counter
            targetCounter = 0;
        }
    }

    private void SetColor(int columnIndex)
    {
        // increment the hue, looping back to 0 if necessary
        m_Hue += (1f / 250f);
        if (m_Hue > 1f)
            m_Hue = 0;
        // Default saturation
        float saturation = .7f;
        // Alternative saturation for even numbers
        if (columnIndex % 2 == 0)
        {
            saturation = .5f;
        }
        // Set color for each child's renderer
        foreach (MeshRenderer renderer in m_ColumnList[columnIndex].GetComponentsInChildren<MeshRenderer>())
        {
            renderer.material.color = Color.HSVToRGB(m_Hue, saturation, .7f);
        }
    }
}
