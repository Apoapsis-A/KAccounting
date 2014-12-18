using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KAccounting
{
	public class ModuleKAccounting : PartModule
	{
		[KSPField]
		public float vesselPartValue;

		[KSPField]
		public float vesselResourceValue;

		[KSPField(guiActive=true, guiFormat="F2", guiName="Vessel Value")]
		public float vesselValue;

		[KSPField(isPersistant=true)]
		public float vesselLaunchedValue;

		[KSPField(isPersistant = true)]
		public int numContractsCompleted;

		[KSPField(isPersistant = true)]
		public float contractFundsEarned;
		[KSPField(isPersistant = true)]
		public float contractReputationEarned;
		[KSPField(isPersistant = true)]
		public float contractScienceEarned;

		[KSPField(isPersistant = true)]
		public float scienceTransmitted;

		void Start()
		{
			print("KAccounting: Start()");
			if (FlightGlobals.fetch == null)
                return;

			if (!HighLogic.LoadedSceneIsFlight)
				return;

			Vessel activeVessel = FlightGlobals.ActiveVessel;

			if (activeVessel == null)
				return;

			if (activeVessel.situation != Vessel.Situations.PRELAUNCH)
				return;

			vesselPartValue = 0.0f;
			vesselResourceValue = 0.0f;

			foreach (Part part in FlightGlobals.ActiveVessel.Parts)
			{
				float dryCost, fuelCost;
				ShipConstruction.GetPartCosts(part.protoPartSnapshot, part.partInfo, out dryCost, out fuelCost);

				vesselPartValue += dryCost;

				foreach (PartResource resource in part.Resources)
				{
					vesselResourceValue += (float)resource.amount * resource.info.unitCost;
				}
			}

			GameEvents.Contract.onCompleted.Add(OnContractCompleted);
			GameEvents.Contract.onParameterChange.Add(OnContractParameterChange);

			GameEvents.OnScienceChanged.Add(OnScienceChanged);

			vesselLaunchedValue = vesselPartValue + vesselResourceValue;
			numContractsCompleted = 0;
			contractFundsEarned = 0.0f;
			contractReputationEarned = 0.0f;
			contractScienceEarned = 0.0f;

			scienceTransmitted = 0.0f;
		}

		private void OnContractCompleted(Contracts.Contract contract)
		{
			contractFundsEarned += (float)contract.FundsCompletion;
			contractReputationEarned += contract.ReputationCompletion;
			contractScienceEarned += contract.ScienceCompletion;
		}

		private void OnContractParameterChange(Contracts.Contract contract, Contracts.ContractParameter contractParam)
		{
			switch (contractParam.State)
			{
				case Contracts.ParameterState.Complete:
					contractFundsEarned += (float)contractParam.FundsCompletion;
					contractReputationEarned += contractParam.ReputationCompletion;
					contractScienceEarned += contractParam.ScienceCompletion;
					break;
				case Contracts.ParameterState.Failed:
					contractFundsEarned += (float)contractParam.FundsFailure;
					contractReputationEarned += contractParam.ReputationFailure;
					break;
			}
		}

		private void OnScienceChanged(float amount, TransactionReasons reason)
		{
			if (reason == TransactionReasons.ScienceTransmission)
			{
				scienceTransmitted += amount;
			}
		}

		void Inactive()
		{
			print("KAccounting: Inactive()");
			GameEvents.Contract.onCompleted.Remove(OnContractCompleted);
			GameEvents.Contract.onParameterChange.Remove(OnContractParameterChange);

			GameEvents.OnScienceChanged.Remove(OnScienceChanged);
		}

		void Update()
		{

            if (FlightGlobals.fetch == null)
                return;

			Vessel activeVessel = FlightGlobals.ActiveVessel;

			if (activeVessel == null)
				return;

			vesselPartValue = 0.0f;
			vesselResourceValue = 0.0f;

			foreach (Part part in FlightGlobals.ActiveVessel.Parts)
			{
				float dryCost, fuelCost;
				ShipConstruction.GetPartCosts(part.protoPartSnapshot, part.partInfo, out dryCost, out fuelCost);

				vesselPartValue += dryCost;

				foreach (PartResource resource in part.Resources)
				{
					vesselResourceValue += (float)resource.amount * resource.info.unitCost;
				}
			}

			vesselValue = vesselPartValue + vesselResourceValue;

			

		}
	}
}
