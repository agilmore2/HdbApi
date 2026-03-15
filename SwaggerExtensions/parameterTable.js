// Custom parameter table UI for HDB API
(function() {
    'use strict';

    // Wait for Swagger UI to load
    function initParameterTable() {
        if (typeof window.ui === 'undefined') {
            setTimeout(initParameterTable, 100);
            return;
        }

        // Use MutationObserver to watch for parameter tables
        const observer = new MutationObserver(function(mutations) {
            mutations.forEach(function(mutation) {
                if (mutation.type === 'childList') {
                    const tables = document.querySelectorAll('table.parameters');
                    tables.forEach(function(table) {
                        customizeParameterTable(table);
                    });
                }
            });
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });

        // Also customize existing tables
        const existingTables = document.querySelectorAll('table.parameters');
        existingTables.forEach(function(table) {
            customizeParameterTable(table);
        });
    }

    function customizeParameterTable(table) {
        const headers = table.querySelectorAll('thead th');
        if (headers.length >= 3) {
            // Assuming headers are: Name, Description, Schema
            // Change to: Name, In, Type, Description
            if (headers[0].textContent.trim() === 'Name' &&
                headers[1].textContent.trim() === 'Description' &&
                headers[2].textContent.trim() === 'Schema') {

                // Insert new 'In' column header after Name
                const inHeader = document.createElement('th');
                inHeader.textContent = 'In';
                inHeader.style.width = '60px';
                headers[0].parentNode.insertBefore(inHeader, headers[1]);

                // Change Schema to Type
                headers[3].textContent = 'Type'; // Now it's the 4th header

                // Add 'Value' header or modify the input area
                // The input is in tbody, not thead
            }
        }

        // Customize the tbody rows
        const rows = table.querySelectorAll('tbody tr');
        rows.forEach(function(row) {
            const cells = row.querySelectorAll('td');
            if (cells.length >= 3) {
                // Assuming cells are: Name, Description, Schema
                // Insert 'In' cell after Name
                const inCell = document.createElement('td');
                inCell.textContent = 'query'; // Since all are query parameters
                inCell.style.fontSize = '12px';
                inCell.style.color = '#666';
                cells[0].parentNode.insertBefore(inCell, cells[1]);

                // The Schema cell becomes Type
                // Description stays
            }
        });
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initParameterTable);
    } else {
        initParameterTable();
    }
})();