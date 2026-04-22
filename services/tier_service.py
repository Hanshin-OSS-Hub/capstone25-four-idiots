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


def split_tier_name(tier_name):
    normalized = normalize_tier_name(tier_name)
    if not normalized:
        return {"stage": None, "base": None, "stage_index": None, "base_index": None}
    try:
        stage_name, base_name = normalized.split(" ", 1)
    except ValueError:
        return {"stage": None, "base": normalized, "stage_index": None, "base_index": None}

    return {
        "stage": stage_name,
        "base": base_name,
        "stage_index": TIER_STAGES.index(stage_name) if stage_name in TIER_STAGES else None,
        "base_index": BASE_TIERS.index(base_name) if base_name in BASE_TIERS else None,
    }
