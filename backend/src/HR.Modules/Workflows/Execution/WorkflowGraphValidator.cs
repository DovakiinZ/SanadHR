using HR.Domain.Engines.FlowBuilder;

namespace HR.Modules.Workflows.Execution;

/// <summary>A single step reduced to just what graph validation needs.</summary>
public record WorkflowGraphNode(Guid Id, WorkflowStepType Type, string Name, Guid? NextSuccess, Guid? NextFailure);

public record GraphValidationResult
{
    public List<string> Errors { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
    public bool IsValid => Errors.Count == 0;
}

/// <summary>
/// Validates a workflow step graph: dangling pointers, a valid root, no circular references, and
/// that every branch terminates. This is the server-side mirror of the builder's live validation,
/// so an invalid graph can never be persisted regardless of the client.
/// </summary>
public interface IWorkflowGraphValidator
{
    GraphValidationResult Validate(Guid? rootStepId, IReadOnlyCollection<WorkflowGraphNode> steps);
}

public class WorkflowGraphValidator : IWorkflowGraphValidator
{
    public GraphValidationResult Validate(Guid? rootStepId, IReadOnlyCollection<WorkflowGraphNode> steps)
    {
        var result = new GraphValidationResult();
        if (steps.Count == 0)
            return result; // empty draft is allowed

        var byId = steps.ToDictionary(s => s.Id);

        // 1. Root must be set and reference an existing step.
        if (rootStepId is null)
            result.Errors.Add("Root step is not set.");
        else if (!byId.ContainsKey(rootStepId.Value))
            result.Errors.Add("Root step references a step that does not exist.");

        // 2. Every non-null pointer must reference an existing step, and no step may point to itself.
        foreach (var s in steps)
        {
            foreach (var (next, branch) in new[] { (s.NextSuccess, "success"), (s.NextFailure, "failure") })
            {
                if (next is null) continue;
                if (next == s.Id)
                    result.Errors.Add($"Step \"{s.Name}\" points to itself on the {branch} branch.");
                else if (!byId.ContainsKey(next.Value))
                    result.Errors.Add($"Step \"{s.Name}\" has a {branch} branch to a step that does not exist.");
            }
        }

        // 3. Cycle detection across the whole graph (white/gray/black DFS).
        if (DetectsCycle(steps, byId))
            result.Errors.Add("Circular reference detected — a step can be reached from itself.");

        // 4. Reachability + end-state checks (only meaningful when the structure is otherwise sound).
        if (result.IsValid && rootStepId is not null)
        {
            var reachable = Reachable(rootStepId.Value, byId);

            foreach (var s in steps)
                if (!reachable.Contains(s.Id))
                    result.Warnings.Add($"Step \"{s.Name}\" is unreachable from the root step.");

            // A branch "ends" when its pointer is null or leads to an End step. Acyclicity already
            // guarantees termination; we additionally warn if an End-typed step has outgoing pointers.
            foreach (var s in steps.Where(s => s.Type == WorkflowStepType.End))
                if (s.NextSuccess is not null || s.NextFailure is not null)
                    result.Warnings.Add($"End step \"{s.Name}\" should not have outgoing branches.");
        }

        return result;
    }

    private static bool DetectsCycle(IReadOnlyCollection<WorkflowGraphNode> steps, Dictionary<Guid, WorkflowGraphNode> byId)
    {
        var state = new Dictionary<Guid, int>(); // 0=white, 1=gray, 2=black

        bool Visit(Guid id)
        {
            state[id] = 1;
            if (byId.TryGetValue(id, out var node))
            {
                foreach (var next in new[] { node.NextSuccess, node.NextFailure })
                {
                    if (next is null || !byId.ContainsKey(next.Value)) continue;
                    var s = state.GetValueOrDefault(next.Value);
                    if (s == 1) return true;                // back-edge to a gray node => cycle
                    if (s == 0 && Visit(next.Value)) return true;
                }
            }
            state[id] = 2;
            return false;
        }

        foreach (var s in steps)
            if (state.GetValueOrDefault(s.Id) == 0 && Visit(s.Id))
                return true;
        return false;
    }

    private static HashSet<Guid> Reachable(Guid root, Dictionary<Guid, WorkflowGraphNode> byId)
    {
        var seen = new HashSet<Guid>();
        var stack = new Stack<Guid>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var id = stack.Pop();
            if (!seen.Add(id) || !byId.TryGetValue(id, out var node)) continue;
            if (node.NextSuccess is { } ok) stack.Push(ok);
            if (node.NextFailure is { } fail) stack.Push(fail);
        }
        return seen;
    }
}
