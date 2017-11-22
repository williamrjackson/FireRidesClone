using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SwingMechanic : MonoBehaviour {
    public float speed = 20;
    public Text scoreOutput;

    private bool m_bIsSwinging;
    private LineRenderer m_Line;
    private Vector3 m_RopeDest;
    private Rigidbody m_Body;
    private bool m_bIsDead;
    private int m_Score;
    private bool m_bHasStarted;

	// Use this for initialization
	void Start () {
        m_Body = GetComponent<Rigidbody>();
        m_Body.useGravity = false;
        m_Line = GetComponent<LineRenderer>();
        m_Line.enabled = true;
        m_bIsDead = false;
        m_Score = 0;
        m_bHasStarted = false;
        // Get the point directly above as pivot point for initial swing
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.up, out hit))
        {
            m_RopeDest = hit.point;
        }
        // Start swinging
        StartCoroutine(InitialSwing());
	}
	
	// Update is called once per frame
	void Update () {
        // Space pressed
		if (Input.GetKeyDown(KeyCode.Space))
        {
            // Set swinging state, in which we drive rather than gravity/physics
            m_bIsSwinging = true;
            m_Line.enabled = true;
            m_Body.useGravity = false;
            if (!m_bHasStarted)
            {
                // InitialSwing coroutine is watching this.
                // mark HasStarted to discontinue swing
                m_bHasStarted = true;
            }
            else
            {
                // Cast a ray to forward/up
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward + transform.up, out hit))
                {
                    // Record where to shoot line, our latest pivot point
                    m_RopeDest = hit.point;
                }
            }
        }
        // Space released
        if (Input.GetKeyUp(KeyCode.Space))
        {
            // Exit swinging state, enable gravity
            m_bIsSwinging = false;
            m_Body.useGravity = true;
            m_Line.enabled = false;
        }
        // If we're swinging...
        else if (m_bIsSwinging || !m_bHasStarted)
        {
            // Draw the line
            m_Line.positionCount = 2;
            m_Line.SetPosition(0, transform.position);
            m_Line.SetPosition(1, m_RopeDest);

            // Push the rigidbody around the pivot point
            m_Body.velocity = Vector3.Cross(-transform.right, (transform.position - m_RopeDest).normalized) * speed;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        // Dead. Reload scene.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        m_bIsDead = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        // Targets are triggers, record score
        // TODO: Move this to game manager; Create target script; increment bullseye value on consecutive hits; detect missed targets; 
        if (other.name == "Bullseye")
        {
            print("Bullseye!");
            m_Score += 2;
        }
        else if(other.name == "OuterTarget")
        {
            print("Outer!");
            m_Score += 1;
        }
        // Kill other colliders to avoid erronious re-triggers
        foreach (Collider collider in other.transform.parent.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }
        // Explode the hit target
        StartCoroutine(ExplodeTarget(other.transform));
        // report new score
        scoreOutput.text = "Score: " + m_Score.ToString();
    }

    // Scale target over time
    private IEnumerator ExplodeTarget(Transform target)
    {
        float duration = .2f;
        float elapsedTime = 0f;
        float startTime = Time.time;    
        Vector3 initialScale = target.localScale;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            target.localScale = Vector3.Lerp(initialScale, Vector3.one * 20, (elapsedTime / duration));
            yield return new WaitForEndOfFrame();
        }
    }
    
    // Pingpong speed to swing the sphere
    private IEnumerator InitialSwing()
    {
        // Remember the original speed value
        float initialSpeed = speed;
        float elapsedTime = 0;
        while(!m_bHasStarted)
        {
            // Change speed value
            speed = Mathf.PingPong(elapsedTime * initialSpeed, initialSpeed * 2) - initialSpeed;
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        // Done swinging. Restore speed value
        speed = initialSpeed;
    }
}
