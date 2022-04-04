using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Rocket : MonoBehaviour
{
    [SerializeField] Text energyText;
    [SerializeField] int energyTotal = 500;
    [SerializeField] int energyApply = 250;
    [SerializeField] float rotSpeed = 100f;
    [SerializeField] float flySpeed = 100f;
    [SerializeField] AudioClip flySound;
    [SerializeField] AudioClip boomSound;
    [SerializeField] AudioClip finishSound;
    [SerializeField] ParticleSystem flyParticles;
    [SerializeField] ParticleSystem boomParticles;
    [SerializeField] ParticleSystem finishParticles;

    bool collisionOff = false;
    Rigidbody rigitBody;
    AudioSource audioSource;
    enum State {Playing, Dead, NextLevel};
    State state = State.Playing;

    // Start is called before the first frame update
    void Start()
    {
        energyText.text = energyTotal.ToString();
        state = State.Playing;
        rigitBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Playing) {
            Launch();
            Rotation();
        }
        DebugKeys();
    }
    void DebugKeys() {
        if (Input.GetKeyDown(KeyCode.L)) {
            LoadNextLevel();
        }
        else if(Input.GetKeyDown(KeyCode.C)) {
            collisionOff = !collisionOff;
        }
    }

    void OnCollisionEnter(Collision collision) {

        if (state == State.Dead || state == State.NextLevel || collisionOff)
            return;

        switch(collision.gameObject.tag) {
            case "Friendly":
                break;
            case "Finish":
                Finish();
                break;
            case "Battery":
                PlusEnergy(1000, collision.gameObject);
                break;
            default:
                Lose();
                break;
        }
    }

    void PlusEnergy(int energyToAdd, GameObject batteryObject) {
        Destroy(batteryObject);
        batteryObject.GetComponent<BoxCollider>().enabled = false;
        energyTotal += energyToAdd;
        energyText.text = energyTotal.ToString();
    }

    void Lose() {
        state = State.Dead;
        audioSource.Stop();
        audioSource.PlayOneShot(boomSound);
        boomParticles.Play();
        //Invoke("LoadFirstLevel", 2f);
        Invoke("LoadThisLevel", 2f);
    }

    void Finish() {
        state = State.NextLevel;
        audioSource.Stop();
        audioSource.PlayOneShot(finishSound);
        finishParticles.Play();
        Invoke("LoadNextLevel", 2f);
    }

    void LoadNextLevel() {
        int currenttLevelIndex = SceneManager.GetActiveScene().buildIndex;
        int nextLevelIndex = currenttLevelIndex + 1;

        if (nextLevelIndex == SceneManager.sceneCountInBuildSettings)
            nextLevelIndex = 0;

        SceneManager.LoadScene(nextLevelIndex);
    }

    void LoadThisLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    void LoadFirstLevel() {
        SceneManager.LoadScene(1);
    }

    void Rotation() {

        float rotationSpeed = rotSpeed * Time.deltaTime;

        rigitBody.freezeRotation = true;
        if (Input.GetKey(KeyCode.A)) {
            transform.Rotate(Vector3.forward * rotationSpeed);
        }
        else if (Input.GetKey(KeyCode.D)) {
            transform.Rotate(-Vector3.forward * rotationSpeed);
        }
        rigitBody.freezeRotation = false;
    }

    void Launch() {
        if (Input.GetKey(KeyCode.W) && energyTotal > 0) {

            energyTotal -= Mathf.RoundToInt(energyApply * Time.deltaTime);
            //energyTotal -= energyApply * Time.deltaTime;
            energyText.text = energyTotal.ToString();
            rigitBody.AddRelativeForce(Vector3.up * flySpeed * Time.deltaTime);

            if (!audioSource.isPlaying) {
                audioSource.PlayOneShot(flySound);
                flyParticles.Play();
            }
            //flyParticles.Play();
        }
        else {
            audioSource.Pause();
            flyParticles.Stop();
        }
        
    }
}
