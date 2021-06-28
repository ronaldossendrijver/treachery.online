/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Client.GenericComponents
{
    public enum SelectComponentLayout : int
    {
        None = 0,

        /// <summary>
        /// This SelectComponent must be grouped with other SelectComponents within a DIV class="form-row"
        /// </summary>
        MultipleInputsPerLineLabelsAbove = 10,

        OneInputPerLineLabelLeft = 20,

        OneInputPerLineLabelAbove = 30
    }

    
}
