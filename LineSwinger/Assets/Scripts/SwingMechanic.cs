using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SwingMechanic : MonoBehaviour {
    public float speed = 20;
    public Text scoreOutput;

    private bool m_bIsSwinging;
    private LineRenderer line;
    private Vector3 m_RopeDest;
    private Rigidbody m_Body;
    private bool m_bIsDead;
    private int m_Score;
    private bool m_bHasStarted;
	// Use this for initialization
	void Start () {
        m_Body = GetComponent<Rigidbody>();
        m_Body.useGravity = false;
        line = GetComponent<LineRenderer>();
        line.enabled = true;
        m_bIsDead = false;
        m_Score = 0;
        m_bHasStarted = false;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.up, out hit))
        {
            m_RopeDest = hit.point;
        }
        StartCoroutine(InitialSwing());
	}
	
	// Update is called once per frame
	void Update () {
        // Space pressed
		if (Input.GetKeyDown(KeyCode.Space))
        {
            m_bIsSwinging = true;
            line.enabled = true;
            m_Body.useGravity = false;
            if (!m_bHasStarted)
            {
                m_bHasStarted = true;
            }
            else
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward + transform.up, out hit))
                {
                    m_RopeDest = hit.point;
                }
            }
        }
        // Space released
        if (Input.GetKeyUp(KeyCode.Space))
        {
            m_bIsSwinging = false;
            m_Body.useGravity = true;
            line.enabled = false;
        }
        else if (m_bIsSwinging || !m_bHasStarted)
        {
            line.positionCount = 2;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, m_RopeDest);

            m_Body.velocity = Vector3.Cross(-transform.right, (transform.position - m_RopeDest).normalized) * speed;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        m_bIsDead = true;
    }
    private void OnTriggerEnter(Collider other)
    {
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
        foreach (Collider collider in other.transform.parent.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }
        StartCoroutine(ExplodeTarget(other.transform));
        scoreOutput.text = "Score: " + m_Score.ToString();
    }

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
    private IEnumerator InitialSwing()
    {
        float initialSpeed = speed;
        float startTime = 0;
        while(!m_bHasStarted)
        {
            speed = Mathf.PingPong(startTime * initialSpeed, initialSpeed * 2) - initialSpeed;
            startTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        speed = initialSpeed;
    }
}
