using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class LevelGeneratorWindow : EditorWindow
{
    private GameObject characterPrefab;
    private GameObject platformPartPrefab;
    private List<GameObject> obstaclePrefabs = new List<GameObject>();
    private int obstacleCount;
    private int platformCount;
    private List<GameObject> generatedObjects = new List<GameObject>();

    private Color playerColor = Color.white;
    private Color platformColor = Color.white;
    private Color obstacleColor = Color.white; //

    private bool randomizeObstacles = false;

    [MenuItem("Tools/Level Generator")]
    public static void ShowWindow()
    {
        GetWindow<LevelGeneratorWindow>("Level Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Level Generator", EditorStyles.boldLabel);

        characterPrefab = EditorGUILayout.ObjectField("Character Prefab", characterPrefab, typeof(GameObject), true) as GameObject;
        platformPartPrefab = EditorGUILayout.ObjectField("Platform Part Prefab", platformPartPrefab, typeof(GameObject), true) as GameObject;

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Obstacle Prefabs", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        int obstacleCount = EditorGUILayout.IntField("Count", obstaclePrefabs.Count);
        if (obstacleCount != obstaclePrefabs.Count)
        {
            while (obstaclePrefabs.Count < obstacleCount)
            {
                obstaclePrefabs.Add(null);
            }
            while (obstaclePrefabs.Count > obstacleCount)
            {
                obstaclePrefabs.RemoveAt(obstaclePrefabs.Count - 1);
            }
        }
        for (int i = 0; i < obstaclePrefabs.Count; i++)
        {
            obstaclePrefabs[i] = EditorGUILayout.ObjectField($"Obstacle {i + 1}", obstaclePrefabs[i], typeof(GameObject), true) as GameObject;
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        platformCount = EditorGUILayout.IntField("Platform Count", platformCount);

        EditorGUILayout.Space();

        playerColor = EditorGUILayout.ColorField("Player Color", playerColor);
        platformColor = EditorGUILayout.ColorField("Platform Color", platformColor);
        obstacleColor = EditorGUILayout.ColorField("Obstacle Color", obstacleColor);

        EditorGUILayout.Space();

        randomizeObstacles = EditorGUILayout.Toggle("Randomize Obstacles", randomizeObstacles);

        EditorGUILayout.Space();

        if (GUILayout.Button("Randomize Colors"))
        {
            RandomizeColors();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Level"))
        {
            GenerateLevel();
        }

        EditorGUILayout.Space();

        if (generatedObjects.Count > 0)
        {
            EditorGUILayout.LabelField("Generated Objects", EditorStyles.boldLabel);
            foreach (GameObject generatedObject in generatedObjects)
            {
                EditorGUILayout.ObjectField(generatedObject, typeof(GameObject), true);
            }
        }
    }

    private void RandomizeColors()
    {
        playerColor = Random.ColorHSV();
        platformColor = Random.ColorHSV();
        obstacleColor = Random.ColorHSV();
    }

    private void GenerateLevel()
    {
        generatedObjects.Clear();

        if (characterPrefab == null || platformPartPrefab == null || obstaclePrefabs.Count == 0)
        {
            Debug.LogError("Please provide valid prefabs.");
            return;
        }

        // Parent Platform Object
        GameObject platformParent = new GameObject("Platform");
        generatedObjects.Add(platformParent);

        // Platform Parts
        float platformPartWidth = platformPartPrefab.transform.localScale.x;
        float platformPartHeight = platformPartPrefab.transform.localScale.y;

        float totalWidth = platformCount * (obstacleCount + 1) * platformPartWidth;

        for (int i = 0; i < platformCount; i++)
        {
            float platformPosX = i * (obstacleCount + 1) * platformPartWidth - totalWidth / 2f;

            for (int j = 0; j < obstacleCount + 1; j++)
            {
                float platformPosY = j * platformPartHeight;

                Vector3 platformPartPosition = new Vector3(platformPosX, platformPosY, 0f);
                GameObject platformPart = Instantiate(platformPartPrefab, platformPartPosition, Quaternion.identity);
                generatedObjects.Add(platformPart);
                platformPart.transform.parent = platformParent.transform;

                Renderer platformRenderer = platformPart.GetComponent<Renderer>();
                if (platformRenderer != null)
                {
                    Material newMaterial = new Material(Shader.Find("Standard"));
                    newMaterial.color = platformColor;
                    platformRenderer.sharedMaterial = newMaterial;
                }

                if (i == 0 && j == 0)
                {
                    // First platform part, place the character
                    float characterPosY = platformPartPosition.y + platformPartHeight + characterPrefab.transform.localScale.y / 2f;
                    Vector3 characterPosition = new Vector3(platformPosX, characterPosY, 0f);
                    GameObject character = Instantiate(characterPrefab, characterPosition, Quaternion.identity);
                    generatedObjects.Add(character);
                    character.transform.parent = platformParent.transform;

                    Renderer characterRenderer = character.GetComponent<Renderer>();
                    if (characterRenderer != null)
                    {
                        Material newMaterial = new Material(Shader.Find("Standard"));
                        newMaterial.color = playerColor;
                        characterRenderer.sharedMaterial = newMaterial;
                    }
                }
            }
        }

        // Obstacles
        float obstacleOffset = totalWidth / (obstacleCount + 1);

        for (int i = 0; i < platformCount-1; i++)
        {
            float platformPosX = i * (obstacleCount + 1) * platformPartWidth - totalWidth / 2f;
            float platformPosY = platformPartHeight / 2f;

            for (int j = 0; j <= obstacleCount + 1; j++)
            {
                if (j != 0) // Ýlk platform parçasý engel deðil
                {
                    float obstaclePosX;
                    float obstaclePosZ;

                    if (randomizeObstacles)
                    {
                        obstaclePosX = Random.Range(-platformPartWidth / 2f, platformPartWidth / 2f) + platformPosX + platformPartWidth;
                        obstaclePosZ = Random.Range(-platformPartWidth / 2f, platformPartWidth / 2f);
                    }
                    else
                    {
                        obstaclePosX = (j - 1) * obstacleOffset + platformPosX + platformPartWidth;
                        obstaclePosZ = 0f;
                    }

                    Vector3 obstaclePosition = new Vector3(obstaclePosX, platformPosY, obstaclePosZ);
                    GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];
                    GameObject obstacle = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity);
                    generatedObjects.Add(obstacle);
                    obstacle.transform.parent = platformParent.transform;

                    Renderer obstacleRenderer = obstacle.GetComponent<Renderer>();
                    if (obstacleRenderer != null)
                    {
                        Material newMaterial = new Material(Shader.Find("Standard"));
                        newMaterial.color = obstacleColor;
                        obstacleRenderer.sharedMaterial = newMaterial;
                    }
                }
            }
        }
        Debug.Log("Level generated.");
    }
}