package de.unibremen.informatik.vcs2see;

import de.unibremen.informatik.st.libvcs4j.RevisionRange;
import lombok.Getter;

import java.io.IOException;
import java.util.Optional;

public class Vcs2See {

    @Getter
    private static ConsoleManager consoleManager;

    @Getter
    private static PropertiesManager propertiesManager;

    @Getter
    private static CodeAnalyser codeAnalyser;

    @Getter
    private static RepositoryCrawler repositoryCrawler;

    @Getter
    private static GraphModifier graphModifier;

    public Vcs2See() throws IOException {
        propertiesManager = new PropertiesManager();
        propertiesManager.loadProperties();

        consoleManager = new ConsoleManager();
        consoleManager.printWelcome();
        consoleManager.printSeparator();

        codeAnalyser = new CodeAnalyser();

        repositoryCrawler = new RepositoryCrawler();

        graphModifier = new GraphModifier();
    }

    private void setup() throws IOException {
        consoleManager.print("SETUP\nYou can set a new value or accept the current one by \npressing <Enter>. To skip the setup you can set the start \nargument \"-Dci=true\". In CI mode no manual interactions \nare necessary.");
        consoleManager.printSeparator();

        setupEnvironment();
        setupRepository();
        setupBase();
        setupAnalysis();

        propertiesManager.saveProperties();
    }

    private void setupEnvironment() throws IOException {
        read("environment.bauhaus");
        read("environment.cpfcsv2rfg");
        consoleManager.printSeparator();
    }

    private void setupRepository() throws IOException {
        read("repository.name");
        read("repository.path");
        read("repository.language");
        read("repository.type");
        consoleManager.printSeparator();
    }

    private void setupBase() throws IOException {
        read("project.base");//TODO: CHECK PATHS
        consoleManager.printSeparator();
    }

    private void setupAnalysis() throws IOException {
        // Amount of analysis commands
        consoleManager.print("Number of commands needed for the analysis. The commands are queried afterwards.");
        Integer commands = null;
        do {
            try {
                commands = Integer.parseInt(consoleManager.readLine("commands="));
            } catch (NumberFormatException ignored) {
                consoleManager.print("The value must be a number.");
            }
        } while (commands == null);
        consoleManager.printSeparator();

        // Remove existing analyser commands
        for(int i = 0; true; i++) {
            String key = "analyser." + i + ".command";
            Optional<String> optional = propertiesManager.getProperty(key);

            if(optional.isEmpty()) {
                break;
            }

            propertiesManager.removeProperty(key);
        }

        // Query analysis commands one by one
        for(int i = 0; i < commands; i++) {
            read("analyser." + i + ".directory");
            read("analyser." + i + ".command");
            consoleManager.printSeparator();
        }
    }

    private void read(String key) throws IOException {
        Optional<String> value = propertiesManager.getProperty(key);
        consoleManager.print("Current value: " + value.orElse("<empty>"));
        String newValue = consoleManager.readLine(key + "=");
        propertiesManager.setProperty(key, newValue);
    }

    public static void main(String[] args) throws IOException {
        Vcs2See vcs2See = new Vcs2See();
        if(!Boolean.parseBoolean(System.getProperty("ci", "false"))) {
            vcs2See.setup();
        } else {
            consoleManager.print("SETUP\nProgram was started in CI mode. The setup is skipped and \nsettings are read from the file. No manual intervention \nis necessary.");
            consoleManager.printSeparator();
        }

        int i = 1;
        Optional<RevisionRange> optional;
        while ((optional = repositoryCrawler.nextRevision()).isPresent()) {
            codeAnalyser.analyse(i++);

            RevisionRange revisionRange = optional.get();
            graphModifier.process(revisionRange);
        }

        consoleManager.print("Program finished.");
        consoleManager.printSeparator();
    }

}
