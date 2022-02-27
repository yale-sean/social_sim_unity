using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SEAN.Editor
{
    [CustomEditor(typeof(SEAN))]
    public class SEANEditor : UnityEditor.Editor
    {
        SEAN script;
        GameObject scriptObject;
        Tasks.Base currentTask;

        SerializedProperty AgentControllerProperty;
        SerializedProperty ControlledAgentProperty;
        SerializedProperty TopDownViewOnlyProperty;
        SerializedProperty TaskCompletionDistanceProperty;

        void OnEnable()
        {
            script = (SEAN)target;
            scriptObject = script.gameObject;

            AgentControllerProperty = serializedObject.FindProperty("AgentController");
            ControlledAgentProperty = serializedObject.FindProperty("ControlledAgent");
            TopDownViewOnlyProperty = serializedObject.FindProperty("TopDownViewOnly");
        }

        public override void OnInspectorGUI()
        {

            string env = GameObject.Find("/Environment").transform.GetChild(0).gameObject.name;

            // Scenario Selection
            int selectedScenarioIndex;
            List<string> scenarios;
            script.UIGetPedestrianBehaviors(out scenarios, out selectedScenarioIndex);
            int scenarioResult = EditorGUILayout.Popup("Pedestrian Control", selectedScenarioIndex, scenarios.ToArray());
            if (selectedScenarioIndex != scenarioResult) {
                script.SetPedestrianBehavior(scenarios[scenarioResult]);
            }

            // Task Selection
            int selectedTaskIndex;
            List<string> tasks;
            script.UIGetTasks(out tasks, out selectedTaskIndex);
            //Debug.Log("selected " + selectedTaskIndex);
            //if (selectedTaskIndex == -1) { selectedTaskIndex = 0; }
            int taskResult = EditorGUILayout.Popup("Robot Task", selectedTaskIndex, tasks.ToArray());
            //Debug.Log(scenarios[selectedScenarioIndex] + " 1 Selected task: " + tasks[taskResult]);
            if (selectedTaskIndex == -1 || selectedTaskIndex != taskResult) {
                if (selectedTaskIndex == -1) {
                    selectedTaskIndex = 0;
                    taskResult = 0;
                }
                script.SetTask(tasks[taskResult]);
            }
            currentTask = script.robotTask;
            //Debug.Log(scenarios[selectedScenarioIndex] + ", all tasks: " + tasks.ToArray() + ", selected task: " + tasks[taskResult]);

            if (tasks[selectedTaskIndex] == "Handcrafted") {
                SerializedObject handcraftedSerializedObject = new SerializedObject(GameObject.Find("RobotTasks/Handcrafted").GetComponent<Tasks.Handcrafted>());
                SerializedProperty HandcraftedScenarioProperty = handcraftedSerializedObject.FindProperty("socialSituation");

                handcraftedSerializedObject.Update();
                EditorGUILayout.PropertyField(HandcraftedScenarioProperty);
                handcraftedSerializedObject.ApplyModifiedProperties();
            }
            //Debug.Log(scenarios[selectedScenarioIndex] +" 3 Selected task: " + tasks[taskResult]);

            serializedObject.Update();
            EditorGUILayout.PropertyField(AgentControllerProperty);
            EditorGUILayout.PropertyField(ControlledAgentProperty);
            EditorGUILayout.PropertyField(TopDownViewOnlyProperty);
            serializedObject.ApplyModifiedProperties();


            SerializedObject taskCompletionSerializedObject = new SerializedObject(currentTask);
            TaskCompletionDistanceProperty = taskCompletionSerializedObject.FindProperty("completionDistance");
            taskCompletionSerializedObject.Update();
            EditorGUILayout.PropertyField(TaskCompletionDistanceProperty);
            taskCompletionSerializedObject.ApplyModifiedProperties();
        }
    }
}
