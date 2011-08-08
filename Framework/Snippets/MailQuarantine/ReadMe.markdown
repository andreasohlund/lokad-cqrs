MailQuarantine
==============

This quarantine sample uses in-memory quarantine to detect repetitive 
processing failures. It records each failure into the streaming container 
(which could be file/Azure) and then allows to retry message processing.

When failure threshold is breached, we send an email to the hardcoded address.
Failure email contains all information about failures and message contents