using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class StatisticsWindow : EditorWindow
{
    [MenuItem("Window/Total statistics")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(StatisticsWindow));

    }
    List<string> options = new List<string>
    {
        "Total Statistics",
        "Current Session Statistics"
    };

    [SerializeField] private int m_SelectedIndex = -1;
    private VisualElement m_RightPane;
    CurrentSession testWindow;
    Total totalWindow;
    private void CreateGUI()
    {
        testWindow = new CurrentSession();
        totalWindow = new Total();
        var root = rootVisualElement;
        var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
        root.Add(splitView);

        var leftPane = new ListView();
        leftPane.style.backgroundColor = new Color(0, 0, 0, 0.45f);
        splitView.Add(leftPane);
        m_RightPane = new VisualElement();

        splitView.Add(m_RightPane);

        leftPane.makeItem = () =>
        {
            var container = new VisualElement
            {
                style = {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.FlexStart,
                alignItems = Align.Center,
                height = 30
                }
            };

            var label = new Label
            {
                style = { unityTextAlign = TextAnchor.MiddleCenter }
            };

            container.Add(label);
            return container;
        };

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.humsteri.stats-monitor/Editor/styles.uss");
        leftPane.bindItem = (item, index) =>
        {
            var label = item.Q<Label>();
            label.text = options[index];
        };
        
        leftPane.itemsSource = options;
        leftPane.fixedItemHeight = 30;
        
        leftPane.selectionChanged += (items) => { m_SelectedIndex = leftPane.selectedIndex; };
        leftPane.selectionChanged += OnSelectionChanged;
        leftPane.selectedIndex = m_SelectedIndex;
    }

    private void OnSelectionChanged(IEnumerable<object> items)
    {
        m_RightPane.Clear(); // Clear previous content
        
        switch (m_SelectedIndex)
        {
            case 0:
                m_RightPane.Add(totalWindow);
                break;
            case 1:
                m_RightPane.Add(testWindow);
                break;
            case 2:

                break;
            default:
                break;
        }
    }
}
