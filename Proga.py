import os
import yaml

class RoleTimeRequirement(yaml.YAMLObject):
    yaml_tag = u'!type:RoleTimeRequirement'
    def __init__(self, role, time):
        self.role = role
        self.time = time

def construct_role_time_requirement(loader, node):
    value = loader.construct_mapping(node)
    return RoleTimeRequirement(value['role'], value['time'])

class DepartmentTimeRequirement(yaml.YAMLObject):
    yaml_tag = u'!type:DepartmentTimeRequirement'
    def __init__(self, department, time):
        self.department = department
        self.time = time

def construct_department_time_requirement(loader, node):
    value = loader.construct_mapping(node)
    return DepartmentTimeRequirement(value['department'], value['time'])

class OverallPlaytimeRequirement(yaml.YAMLObject):
    yaml_tag = u'!type:OverallPlaytimeRequirement'
    def __init__(self, hours):
        self.hours = hours

def construct_overall_playtime_requirement(loader, node):
    value = loader.construct_mapping(node)
    time = value['time']
    return int(time)


class AddImplantSpecial(yaml.YAMLObject):
    yaml_tag = u'!type:AddImplantSpecial'
    def __init__(self, implants):
        self.implants = implants

def construct_add_implant_special(loader, node):
    value = loader.construct_mapping(node)
    return AddImplantSpecial(value['implants'])

class AddComponentSpecial(yaml.YAMLObject):
    yaml_tag = u'!type:AddComponentSpecial'
    def __init__(self, components):
        self.components = components

def construct_add_component_special(loader, node):
    value = loader.construct_mapping(node)
    return AddComponentSpecial(value['components'])

def construct_invisible_wall_action_event(loader, node):
    value = loader.construct_scalar(node)
    return {'type': 'InvisibleWallActionEvent', 'value': value}

def construct_invisible_wall_action_event(loader, node):
    value = loader.construct_scalar(node)
    return {'type': 'InvisibleWallActionEvent', 'value': value}

def construct_give_item_on_holiday(loader, node):
    value = loader.construct_mapping(node)
    holiday = value.get('holiday', None)
    prototype = value.get('prototype', None)
    return {'type': 'GiveItemOnHolidaySpecial', 'holiday': holiday, 'prototype': prototype}

# def read_yaml_file(file_path):
#     with open(file_path, 'r') as file:
#         yaml.SafeLoader.add_constructor('!type:RoleTimeRequirement', construct_role_time_requirement)
#         yaml.SafeLoader.add_constructor('!type:DepartmentTimeRequirement', construct_department_time_requirement)
#         yaml.SafeLoader.add_constructor('!type:OverallPlaytimeRequirement', construct_overall_playtime_requirement)
#         yaml.SafeLoader.add_constructor('!type:AddImplantSpecial', construct_add_implant_special)
#         yaml.SafeLoader.add_constructor('!type:AddComponentSpecial', construct_add_component_special)
#         yaml.SafeLoader.add_constructor('!type:InvisibleWallActionEvent', construct_invisible_wall_action_event)
#         yaml.SafeLoader.add_constructor('!type:GiveItemOnHolidaySpecial', construct_give_item_on_holiday)
#         return yaml.safe_load(file)

def read_yaml_file(file_path):
    def construct_type(loader, node):
        value = loader.construct_mapping(node)
        type_name = value.get('type', None)
        if type_name:
            constructor_func = globals().get(f'construct_{type_name.lower()}')
            if constructor_func:
                return constructor_func(loader, node)
            else:
                raise ValueError(f"Constructor for type '{type_name}' not found.")
        else:
            raise ValueError("Type field is missing in the YAML data.")
    with open(file_path, 'r') as file:
        yaml.SafeLoader.add_constructor('!type', construct_type)
        return yaml.safe_load(file)

def find_item_ids(directory, item_ids):
    for root, _, files in os.walk(directory):
        for file_name in files:
            if file_name.endswith('.yml'):
                file_path = os.path.join(root, file_name)
                data = read_yaml_file(file_path)
                if isinstance(data, dict):
                    for key, value in data.items():
                        if key == 'id' and value in item_ids:
                            print(f"Найден айди предмета: {value} в файле: {file_path}")

def process_files(directory):
    for root, _, files in os.walk(directory):
        for file_name in files:
            if file_name.endswith('.yml'):
                file_path = os.path.join(root, file_name)
                data = read_yaml_file(file_path)
                if isinstance(data, dict) and 'type' in data and data['type'] == 'job':
                    job_id = data.get('id')
                    print(f"Айди профессии: {job_id}")

                    if 'startingGear' in data:
                        starting_gear_id = data['startingGear']
                        print(f"Стартовое снаряжение: {starting_gear_id}")

                        item_ids = [value for key, value in data.get('equipment', {}).items()]
                        find_item_ids("Resources/Prototypes/Entities", item_ids)

process_files("Resources/Prototypes/Roles/Jobs")