import os

class ContextCreator:
    """
    A class to create a context for use in a prompt for a language model. 
    It scans directories for files of specific types, collects the content, 
    and writes the final context to a file.
    """

    def __init__(self):
        """
        Initialize the ContextCreator instance, scan directories, start the user interface, 
        and create the context.
        """
        self.context = ""
        self.chosen_dirs = []
        self.all_dirs = []
        self.files_to_be_included = []
        self.include_root_files = False
        self.file_types = [".py", ".txt", ".md", ".yml", ".cs"]  # change to your need
        
        try:
            self.all_dirs = self.scan_for_right_type_directories()
            if not self.all_dirs:
                print("No directories found with files of the specified types.")
                return
            self.start_user_interface()
            self.create_context()
        except Exception as e:
            print(f"Error during initialization: {e}")

    def create_context(self):
        """
        Create the context by appending content from the chosen directories and files.
        """
        try:
            print("Creating context...")
            self.find_files()
            if not self.files_to_be_included:
                print("No files found to include in the context.")
                return
            self.append_files_to_context()
            with open("collected_context.txt", "w", encoding="utf-8") as f:
                f.write(self.context)
            print(f"Context successfully written to collected_context.txt")
        except Exception as e:
            print(f"Error creating context: {e}")

    def scan_for_right_type_directories(self):
        """
        Scan all directories and find those containing files of the specified types.

        Returns:
            list: A list of directories containing files with the specified types.
        """
        all_dirs = []
        for root, dirs, files in os.walk('.'):
            if root.startswith('./venv'):
                continue
            if self.has_right_type_file(root):
                all_dirs.append(root)
        return all_dirs

    def has_right_type_file(self, dir_path):
        """
        Check if a directory contains at least one file with the right type.

        Args:
            dir_path (str): The directory path to check.

        Returns:
            bool: True if the directory contains at least one file with the right type, False otherwise.
        """
        try:
            for file in os.listdir(dir_path):
                if self.is_right_type(file):
                    return True
        except OSError as e:
            print(f"Error accessing directory {dir_path}: {e}")
        return False

    def is_right_type(self, file):
        """
        Check if a file has one of the specified types.

        Args:
            file (str): The file name to check.

        Returns:
            bool: True if the file has the right type, False otherwise.
        """
        return os.path.splitext(file)[1] in self.file_types

    def append_files_to_context(self):
        """
        Append the contents of the files to the context in a formatted way.
        """
        for file in self.files_to_be_included:
            try:
                self.context += f"\n\n{'=' * 5}\n{file}\n{'=' * 5}\n"
                with open(file, 'r', encoding="utf-8") as f:
                    self.context += f.read()
            except (OSError, UnicodeDecodeError) as e:
                print(f"Error reading file {file}: {e}")

    def find_files(self):
        """
        Find all files in the chosen directories and the root directory (if included).
        """
        try:
            for dir in self.chosen_dirs:
                for file in os.listdir(dir):
                    file_path = os.path.join(dir, file)
                    if os.path.isfile(file_path) and self.is_right_type(file):
                        self.files_to_be_included.append(file_path)
            if self.include_root_files:
                for file in os.listdir('.'):
                    file_path = os.path.join('.', file)
                    if os.path.isfile(file_path) and self.is_right_type(file):
                        self.files_to_be_included.append(file_path)
        except OSError as e:
            print(f"Error accessing files: {e}")

    def start_user_interface(self):
        """
        Start the user interface to allow the user to choose directories and include root files.
        """
        try:
            print("Directories with files of the right type:")
            for i, d in enumerate(self.all_dirs, 1):
                print(f"{i}. {d}")
            print("\nChoose directories by number, separated by a space:")
            chosen_dirs = input().split()
            
            # Validate chosen numbers
            for i in chosen_dirs:
                if not i.isdigit() or int(i) < 1 or int(i) > len(self.all_dirs):
                    raise ValueError(f"Invalid choice: {i}")
            
            self.chosen_dirs = [self.all_dirs[int(i) - 1] for i in chosen_dirs]
            self.include_root_files = input("\nInclude files from root directory? (y/n): ").strip().lower() == 'y'
            
            print(f"\nChosen directories: {self.chosen_dirs}")
            print(f"Include files from root directory: {self.include_root_files}")
        except ValueError as e:
            print(f"Input error: {e}")
        except Exception as e:
            print(f"Unexpected error during user input: {e}")

if __name__ == "__main__":
    ContextCreator()


