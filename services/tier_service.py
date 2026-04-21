TIER_STAGES = ["normal", "core", "magic", "rare", "elite", "unique", "legend"]
BASE_TIERS = ["bronze", "silver", "gold", "platinum", "diamond", "master", "challenger"]


def normalize_tier_name(tier_name):
    value = "" if tier_name is None else str(tier_name).strip().lower().replace("_", " ")
    if not value:
        return value
    try:
        stage_name, base_name = value.split(" ", 1)
    except ValueError:
        return value
    if stage_name not in TIER_STAGES or base_name not in BASE_TIERS:
        return value
    return f"{stage_name} {base_name}"



def tier_name_to_api(tier_name):
    normalized = normalize_tier_name(tier_name)
    if not normalized:
        return normalized
    try:
        stage_name, base_name = normalized.split(" ", 1)
    except ValueError:
        return normalized
    return f"{stage_name}_{base_name}"
