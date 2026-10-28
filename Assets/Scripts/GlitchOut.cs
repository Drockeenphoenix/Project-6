using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GlitchOut : MonoBehaviour
{
    private class GlitchOutMats
    {
        public Material[] StartMaterials {
            get;
            private set;
        }
        
        public Material[] GlitchMaterials {
            get;
            private set;
        }
        
        public GlitchOutMats(Material[] startMaterials, Material[] glitchMaterials)
        {
            StartMaterials = startMaterials;
            GlitchMaterials = glitchMaterials;
        }
    }
    
    public bool startGlitch;
    
    [SerializeField] private Material blankMaterial;
    [SerializeField] private float maxGlitchInterval = 2f;
    [SerializeField] private float minGlitchInterval = 2f;
    [SerializeField] private float maxGlitchTime = 30f;
    [SerializeField] private float timeInSceneAfterGlitch = 5f;
    private float currentGlitchTime = 0f;
    private bool currentlyGlitching = false;
    private bool swappingScenes = false;
    
    private List<MeshRenderer> sceneMeshRenderers = new List<MeshRenderer>();
    private Dictionary<MeshRenderer, GlitchOutMats> glitchMatsDict = new Dictionary<MeshRenderer, GlitchOutMats>();
    
    private void Start()
    {
        GetAllMeshes();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) {
            startGlitch = true;
        }

        if (startGlitch) {
            RandomGlitch();
        }
    }

    private void RandomGlitch()
    {
        if (currentGlitchTime >= maxGlitchTime && !swappingScenes) {
            SetMeshesToGlitched();
            StartCoroutine(SwapScenes());
            return;
        }
        currentGlitchTime += Time.deltaTime;

        if (!currentlyGlitching) {
            StartCoroutine(Glitch());
        }
    }
    
    private IEnumerator SwapScenes()
    {
        swappingScenes = true;
        yield return new WaitForSeconds(timeInSceneAfterGlitch);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private IEnumerator Glitch()
    {
        currentlyGlitching = true;
        
        SetMeshesToNormal();
        int objsToGlitch = Mathf.CeilToInt(Mathf.Lerp(1, sceneMeshRenderers.Count, currentGlitchTime / maxGlitchTime));
        float glitchInterval = Mathf.Lerp(maxGlitchInterval, minGlitchInterval, 1.5f * currentGlitchTime / maxGlitchTime);

        GlitchObjects(objsToGlitch);
        
        yield return new WaitForSeconds(glitchInterval);

        currentlyGlitching = false;
    }

    private void GlitchObjects(int objsToGlitch)
    {
        List<MeshRenderer> leftoverRenderers = new List<MeshRenderer>(sceneMeshRenderers);
        
        for (int i = 0; i < objsToGlitch; i++) 
        {
            if (leftoverRenderers.Count == 0) break;
            
            int randomIndex = Random.Range(0, leftoverRenderers.Count);
            MeshRenderer mr = leftoverRenderers[randomIndex];

            mr.materials = glitchMatsDict[mr].GlitchMaterials;
            
            leftoverRenderers.Remove(mr);
        }
    }

    private void SetMeshesToNormal()
    {
        foreach (MeshRenderer mr in sceneMeshRenderers) {
            mr.materials = glitchMatsDict[mr].StartMaterials;
        }
    }

    private void SetMeshesToGlitched()
    {
        foreach (MeshRenderer mr in sceneMeshRenderers) {
            mr.materials = glitchMatsDict[mr].GlitchMaterials;
        }
    }

    private void GetAllMeshes()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        
        List<GameObject> rootObjects = new List<GameObject>();
        currentScene.GetRootGameObjects(rootObjects);
        
        foreach (GameObject rootObject in rootObjects)
        {
            GetLineage(rootObject);
        }
    }

    private void GetLineage(GameObject obj)
    {
        if (obj.GetComponent(typeof(MeshRenderer)) != null) {
            AddMeshRenderer(obj.GetComponent<MeshRenderer>());
        }
        
        foreach (Transform child in obj.transform)
        {
            GetLineage(child.gameObject);
        }
    }

    private void AddMeshRenderer(MeshRenderer mr)
    {
        sceneMeshRenderers.Add(mr);

        Material[] glitchMaterials = new Material[mr.materials.Length];
        for (int i = 0; i < mr.materials.Length; i++) {
            glitchMaterials[i] = blankMaterial;
        }
        
        glitchMatsDict.Add(mr, new GlitchOutMats(mr.materials, glitchMaterials));
    }
}
