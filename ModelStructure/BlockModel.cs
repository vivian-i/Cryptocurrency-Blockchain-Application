namespace ModelStructure
{
    /**
     * BlockModel defines a block object.
     * It contains a block id which is an unsigned integer that uniquely identifies the block,
     * A  wallet ID from which is an unsigned integer that identify the account the transaction is from,
     * A wallet ID to which is an unsigned integer that identify the account the transaction is to,
     * An amount which is a float that represents the amount of coins being sent which cannot be negative,
     * A block offset which is an unsigned integer and is used to produce a valid hash,
     * A previous block hash which is the hash of the block immediately prior to this one, and
     * A hash which is the hash of the current block.
     */
    public class BlockModel
    {
        //public fields
        public uint blockId;
        public uint walletIdFrom;
        public uint walletIdTo;
        public float amount;
        public uint blockOffset;
        public string prevBlockHashStr;
        public string currBlockHashStr;
    }
}
