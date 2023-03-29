//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Admin.Client.Store.Delegation
{
    using System.Collections.Immutable;
    using System.Linq;
    using Fluxor;
    using HealthGateway.Admin.Client.Models;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;

#pragma warning disable CS1591, SA1600
    public static class DelegationReducers
    {
        [ReducerMethod(typeof(DelegationActions.SearchAction))]
        public static DelegationState ReduceSearchAction(DelegationState state)
        {
            return state with
            {
                Search = state.Search with
                {
                    IsLoading = true,
                },
            };
        }

        [ReducerMethod]
        public static DelegationState ReduceSearchSuccessAction(DelegationState state, DelegationActions.SearchSuccessAction action)
        {
            return state with
            {
                Search = state.Search with
                {
                    IsLoading = false,
                    Result = new() { Dependent = action.Dependent, Delegates = action.Delegates },
                    Error = null,
                },
                Dependent = action.Dependent,
                Delegates = action.Delegates.ToImmutableList(),
            };
        }

        [ReducerMethod]
        public static DelegationState ReduceSearchFailAction(DelegationState state, DelegationActions.SearchFailAction action)
        {
            return state with
            {
                Search = state.Search with
                {
                    IsLoading = false,
                    Error = action.Error,
                },
            };
        }

        [ReducerMethod(typeof(DelegationActions.DelegateSearchAction))]
        public static DelegationState ReduceDelegateSearchAction(DelegationState state)
        {
            return state with
            {
                DelegateSearch = state.DelegateSearch with
                {
                    IsLoading = true,
                },
            };
        }

        [ReducerMethod]
        public static DelegationState ReduceDelegateSearchSuccessAction(DelegationState state, DelegationActions.DelegateSearchSuccessAction action)
        {
            return state with
            {
                DelegateSearch = state.DelegateSearch with
                {
                    IsLoading = false,
                    Result = action.Data,
                    Error = null,
                },
            };
        }

        [ReducerMethod]
        public static DelegationState ReduceDelegateSearchFailAction(DelegationState state, DelegationActions.DelegateSearchFailAction action)
        {
            return state with
            {
                DelegateSearch = state.DelegateSearch with
                {
                    IsLoading = false,
                    Error = action.Error,
                },
            };
        }

        [ReducerMethod]
        public static DelegationState ReduceAddDelegateAction(DelegationState state, DelegationActions.AddDelegateAction action)
        {
            IImmutableList<ExtendedDelegateInfo> delegates = state.Delegates;
            if (state.DelegateSearch.Result != null)
            {
                ExtendedDelegateInfo delegateInfo = state.DelegateSearch.Result;
                delegateInfo.StagedDelegationStatus = action.StagedDelegationStatus;
                delegates = delegates.Add(delegateInfo);
            }

            return state with
            {
                Delegates = delegates,
            };
        }

        [ReducerMethod]
        public static DelegationState ReduceSetDisallowedDelegationStatusAction(DelegationState state, DelegationActions.SetDisallowedDelegationStatusAction action)
        {
            IImmutableList<ExtendedDelegateInfo> delegates = state.Delegates
                .Select(
                    d =>
                    {
                        if (d.Hdid == action.Hdid)
                        {
                            d.StagedDelegationStatus = action.Disallow switch
                            {
                                true => DelegationStatus.Disallowed,
                                false when d.DelegationStatus is DelegationStatus.Added or DelegationStatus.Allowed => d.DelegationStatus,
                                _ => DelegationStatus.Allowed,
                            };
                        }

                        return d;
                    })
                .ToImmutableList();

            return state with
            {
                Delegates = delegates,
            };
        }

        [ReducerMethod(typeof(DelegationActions.ProtectDependentAction))]
        public static DelegationState ReduceProtectDependentAction(DelegationState state)
        {
            return state with
            {
                Protect = state.Protect with
                {
                    IsLoading = true,
                },
            };
        }

        [ReducerMethod(typeof(DelegationActions.ProtectDependentSuccessAction))]
        public static DelegationState ReduceProtectDependentSuccessAction(DelegationState state)
        {
            DependentInfo? dependent = state.Dependent;
            if (dependent != null)
            {
                dependent.Protected = true;
            }

            return state with
            {
                Protect = state.Protect with
                {
                    IsLoading = false,
                    Error = null,
                },
                Dependent = dependent,
                Delegates = state.Delegates
                    .Where(d => d.StagedDelegationStatus is not DelegationStatus.Disallowed)
                    .Select(
                        d =>
                        {
                            d.DelegationStatus = d.StagedDelegationStatus;
                            return d;
                        })
                    .ToImmutableList(),
            };
        }

        [ReducerMethod]
        public static DelegationState ReduceProtectDependentFailAction(DelegationState state, DelegationActions.ProtectDependentFailAction action)
        {
            return state with
            {
                Protect = state.Protect with
                {
                    IsLoading = false,
                    Error = action.Error,
                },
            };
        }

        [ReducerMethod(typeof(DelegationActions.UnprotectDependentAction))]
        public static DelegationState ReduceUnprotectDependentAction(DelegationState state)
        {
            return state with
            {
                Unprotect = state.Unprotect with
                {
                    IsLoading = true,
                },
            };
        }

        [ReducerMethod(typeof(DelegationActions.UnprotectDependentSuccessAction))]
        public static DelegationState ReduceUnprotectDependentSuccessAction(DelegationState state)
        {
            DependentInfo? dependent = state.Dependent;
            if (dependent != null)
            {
                dependent.Protected = false;
            }

            return state with
            {
                Unprotect = state.Unprotect with
                {
                    IsLoading = false,
                    Error = null,
                },
                Dependent = dependent,
                Delegates = state.Delegates
                    .Where(d => d.DelegationStatus is not DelegationStatus.Allowed)
                    .ToImmutableList(),
            };
        }

        [ReducerMethod]
        public static DelegationState ReduceUnprotectDependentFailAction(DelegationState state, DelegationActions.UnprotectDependentFailAction action)
        {
            return state with
            {
                Unprotect = state.Unprotect with
                {
                    IsLoading = false,
                    Error = action.Error,
                },
            };
        }

        [ReducerMethod]
        public static DelegationState ReduceSetEditModeAction(DelegationState state, DelegationActions.SetEditModeAction action)
        {
            IImmutableList<ExtendedDelegateInfo> delegates = state.Delegates;
            if (!action.Enabled)
            {
                delegates = delegates
                    .Where(d => d.DelegationStatus is DelegationStatus.Added or DelegationStatus.Allowed)
                    .Select(
                        d =>
                        {
                            d.StagedDelegationStatus = d.DelegationStatus;
                            return d;
                        })
                    .ToImmutableList();
            }

            return state with
            {
                InEditMode = action.Enabled,
                Delegates = delegates,
            };
        }

        [ReducerMethod(typeof(DelegationActions.ClearProtectErrorAction))]
        public static DelegationState ReduceClearAddErrorAction(DelegationState state)
        {
            return state with
            {
                Protect = state.Protect with
                {
                    Error = null,
                },
            };
        }

        [ReducerMethod(typeof(DelegationActions.ClearUnprotectErrorAction))]
        public static DelegationState ReduceClearUpdateErrorAction(DelegationState state)
        {
            return state with
            {
                Unprotect = state.Unprotect with
                {
                    Error = null,
                },
            };
        }

        [ReducerMethod(typeof(DelegationActions.ResetStateAction))]
        public static DelegationState ReduceResetStateAction(DelegationState state)
        {
            return state with
            {
                Search = new(),
                Protect = new(),
                Unprotect = new(),
                Dependent = null,
                Delegates = ImmutableList<ExtendedDelegateInfo>.Empty,
                InEditMode = false,
            };
        }
    }
}
