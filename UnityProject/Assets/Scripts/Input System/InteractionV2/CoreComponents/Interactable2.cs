
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Version of <see cref="Interactable{T}"/> which supports 2
/// interaction types.
///
/// </summary>
/// <typeparamref name="T">Interaction subtype
/// for the interaction that this component wants to handle (such as MouseDrop for a mouse drop interaction).
/// Must be a subtype of Interaction.</typeparamref>
/// /// <typeparamref name="T2">Second interaction subtype
/// for the interaction that this component wants to handle (such as MouseDrop for a mouse drop interaction).
/// Must be a subtype of Interaction.</typeparamref>
public abstract class Interactable<T,T2>
	: Interactable<T>, IInteractable<T2>, IInteractionProcessor<T2>
	where T : Interaction
	where T2 : Interaction
{
	//we delegate our interaction logic to this.
	private InteractionCoordinator<T2> coordinator;

	protected void Start()
	{
		EnsureCoordinatorInit();
		//subclasses must remember to call base.Start() if they use Start
		base.Start();
	}

	private void EnsureCoordinatorInit()
	{
		if (coordinator == null)
		{
			coordinator = new InteractionCoordinator<T2>(this, WillInteractT2, ServerPerformInteraction);
		}
	}

	/// <summary>
	/// Decides if interaction logic should proceed. On client side, the interaction
	/// request will only be sent to the server if this returns true. On server side,
	/// the interaction will only be performed if this returns true.
	///
	/// Each interaction has a default implementation of this which should apply for most cases.
	/// By overriding this and adding more specific logic, you can reduce the amount of messages
	/// sent by the client to the server, decreasing overall network load.
	/// </summary>
	/// <param name="interaction">interaction to validate</param>
	/// <param name="side">which side of the network this is being invoked on</param>
	/// <returns>True/False based on whether the interaction logic should proceed as described above.</returns>
	protected virtual bool WillInteractT2(T2 interaction, NetworkSide side)
	{
		return DefaultWillInteract.Default(interaction, side);
	}

	/// <summary>
	/// Server-side. Called after validation succeeds on server side.
	/// Server should perform the interaction and inform clients as needed.
	/// </summary>
	/// <param name="interaction"></param>
	/// <returns>Currently should always be SOMETHING_HAPPENED, may be expanded later if needed.</returns>
	protected abstract void ServerPerformInteraction(T2 interaction);

	/// <summary>
	/// Client-side prediction. Called after validation succeeds on client side.
	/// Client can perform client side prediction. NOT invoked for server player, since there is no need
	/// for prediction.
	/// </summary>
	/// <param name="interaction"></param>
	protected virtual void ClientPredictInteraction(T2 interaction) { }

	public bool Interact(T2 info)
	{
		EnsureCoordinatorInit();
		return InteractionComponentUtils.CoordinatedInteract(info, coordinator, ClientPredictInteraction);
	}

	public bool ServerProcessInteraction(T2 info)
	{
		EnsureCoordinatorInit();
		return InteractionComponentUtils.ServerProcessCoordinatedInteraction(info, coordinator);
	}
}