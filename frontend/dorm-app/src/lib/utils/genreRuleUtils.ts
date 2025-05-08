import { GenderRule } from "../types/enums";

export const getGenderRuleColor = (genderRule: GenderRule) => {
  switch (genderRule) {
    case GenderRule.Male:
      return "bg-blue-100 text-blue-800";
    case GenderRule.Female:
      return "bg-pink-100 text-pink-800";
    case GenderRule.Mixed:
      return "bg-violet-100 text-violet-800";
    default:
      return "bg-gray-100 text-gray-800";
  }
};
