namespace LearningLab.Data.Models.Campaign.Rules;

public enum CampaignEventType
{
    BooleanFlag,
    SingleChoice,
    TextValue,
    NumericValue
}

public enum ConditionalTargetType
{
    StoryBlock,
    StoryBeat,
    InformationBeat
}

public enum ConditionalRuleEffectType
{
    RequiredForAvailability,
    RequiredForVisibility,
    ExclusivePath,
    OptionalInformation
}

public enum ConditionGroupOperator
{
    And,
    Or,
    ExactlyOne
}

public enum ConditionComparisonOperator
{
    IsSet,
    IsNotSet,
    Equals,
    NotEquals,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual
}

public enum OutcomeSourceType
{
    StoryBlock,
    StoryBeat,
    ChoiceOption,
    DecisionChoice,
    RoleplayingNpcInteraction,
    RoleplayingInformation
}

public enum OutcomeOperationType
{
    Set,
    Clear,
    Increment,
    Decrement
}

public enum CampaignChoiceSelectionMode
{
    Single,
    Multiple,
    ExactlyOne
}
