using Godot;

namespace Aberration;

[GlobalClass]
public partial class Switcheroo : Node {

	public enum SwitchModeEnum {
		Hide,
		Remove,
		HideAndDisable,
	}

	[Export] public Node Node1 { get; set; }
	[Export] public Node Node2 { get; set; }

	[Export] public SwitchModeEnum SwitchMode { get; set; } = SwitchModeEnum.Remove;

	private Node _node1Parent;
	private Node _node2Parent;

	public override void _Ready() {
		_node1Parent = Node1.GetParent();
		_node2Parent = Node2.GetParent();

		switch (SwitchMode) {
			case SwitchModeEnum.Remove: _node2Parent.CallDeferred("remove_child", Node2); break;
			case SwitchModeEnum.Hide: TrySetVisibility(Node2, false); break;
			case SwitchModeEnum.HideAndDisable: {
				TrySetVisibility(Node2, false);
				Node2.ProcessMode = ProcessModeEnum.Disabled;
			}; break;
		}
	}

	public void Switch() {

		(Node2, Node1) = (Node1, Node2);
		(_node2Parent, _node1Parent) = (_node1Parent, _node2Parent);

		switch (SwitchMode) {
			case SwitchModeEnum.Remove: {
				_node1Parent.CallDeferred("add_child", Node1);
				_node2Parent.CallDeferred("remove_child", Node2);
			}; break;
			case SwitchModeEnum.Hide: {
				TrySetVisibility(Node1, true);
				TrySetVisibility(Node2, false);
			}; break;
			case SwitchModeEnum.HideAndDisable: {
				TrySetVisibility(Node1, true);
				Node1.ProcessMode = ProcessModeEnum.Inherit;
				TrySetVisibility(Node2, false);	
				Node2.ProcessMode = ProcessModeEnum.Disabled;
			}; break;
		}		
	}	
	
	private static void TrySetVisibility(Node node, bool visible) {
		if (node is Node3D node3d) node3d.Visible = visible;
		else if (node is CanvasItem canvasItem) canvasItem.Visible = visible;
		else GD.PushWarning($"Node {node.Name} can't be hidden.");
	}

}