namespace HealthGateway.Admin.Client.Store.BlockAccess
{
    using Fluxor;

#pragma warning disable CS1591, SA1600
    public static class BlockAccessReducers
    {
        [ReducerMethod(typeof(BlockAccessActions.SetBlockAccessAction))]
        public static BlockAccessState ReduceSetBlockAccessAction(BlockAccessState state)
        {
            return state with
            {
                BlockAccess = state.BlockAccess with
                {
                    IsLoading = true,
                },
            };
        }

        [ReducerMethod(typeof(BlockAccessActions.SetBlockAccessSuccessAction))]
        public static BlockAccessState ReduceBlockAccessSuccessAction(BlockAccessState state)
        {
            return state with
            {
                BlockAccess = state.BlockAccess with
                {
                    IsLoading = false,
                    Error = null,
                },
            };
        }

        [ReducerMethod]
        public static BlockAccessState ReduceBlockAccessFailureAction(BlockAccessState state, BlockAccessActions.SetBlockAccessFailureAction action)
        {
            return state with
            {
                BlockAccess = state.BlockAccess with
                {
                    IsLoading = false,
                    Error = action.Error,
                },
            };
        }
    }
}
