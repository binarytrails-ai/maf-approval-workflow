"use client";

import React, { useState } from "react";
import "@copilotkit/react-ui/styles.css";
import {
  CopilotKit,
  useCopilotReadable,
  useHumanInTheLoop,
} from "@copilotkit/react-core";
import { CopilotChat } from "@copilotkit/react-ui";

export default function Page() {
  return (
    <CopilotKit
      runtimeUrl="/api/copilotkit"
      showDevConsole={false}
      agent="contoso_agent"
    >
      <Chat />
    </CopilotKit>
  );
}

/**
 * Enhanced Approve / Reject UI with accessibility and loading states
 */
const ApprovalUI = ({
  args,
  respond,
  status,
}: {
  args: { bikeId?: number; amount?: number; approvalId?: string };
  respond?: (value: any) => void;
  status: string;
}) => {
  const [isResponding, setIsResponding] = useState(false);

  // Only show when executing and respond function is available
  if (status !== "executing" || !respond) return null;

  const handleRespond = async (approved: boolean) => {
    setIsResponding(true);
    // Return response object with approval_id and approved fields
    respond({
      approval_id: args.approvalId,
      approved: approved
    });
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    // Prevent keyboard shortcuts if already responding
    if (isResponding) return;
    
    if (e.key === "Escape") {
      e.preventDefault();
      handleRespond(false);
    } else if (e.key === "Enter") {
      e.preventDefault();
      handleRespond(true);
    }
  };

  const formattedAmount = args?.amount?.toFixed(2) || "0.00";

  return (
    <div
      className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4"
      role="dialog"
      aria-labelledby="approval-title"
      aria-describedby="approval-message"
      onKeyDown={handleKeyDown}
      tabIndex={-1}
    >
      <div className="bg-white dark:bg-gray-800 border rounded-xl shadow-lg p-6 max-w-md w-full animate-fadeIn">
        <h3
          id="approval-title"
          className="font-semibold text-lg mb-3 dark:text-white"
        >
          💳 Payment Approval Required
        </h3>
        <div
          id="approval-message"
          className="text-sm mb-6 break-words dark:text-gray-300 space-y-2"
        >
          <p>Please review and approve this payment:</p>
          <div className="bg-gray-50 dark:bg-gray-700 p-4 rounded-lg space-y-1">
            <p>
              <span className="font-medium">Bike ID:</span> {args.bikeId ?? "N/A"}
            </p>
            <p className="text-lg font-semibold text-green-600 dark:text-green-400">
              Total Amount: ${formattedAmount}
            </p>
          </div>
          <p className="text-xs text-gray-500 dark:text-gray-400">
            Press Enter to approve or Escape to reject
          </p>
        </div>

        <div className="flex justify-end gap-3">
          <button
            className="px-4 py-2 rounded bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 dark:text-white disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            onClick={() => handleRespond(false)}
            disabled={isResponding}
            aria-label="Reject action"
          >
            ✗ Reject
          </button>

          <button
            className="px-4 py-2 rounded bg-green-600 text-white hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            onClick={() => handleRespond(true)}
            disabled={isResponding}
            aria-label="Approve action"
          >
            {isResponding ? "Processing..." : "✓ Approve Payment"}
          </button>
        </div>
      </div>
    </div>
  );
};

const Chat = () => {
  /**
   * Provide readable context to the agent
   */
  useCopilotReadable({
    description: "Contoso Bikestore customer",
    value: "User browsing bikes",
  });

  /**
   * 🔥 Human-in-the-loop approval tool
   * Based on: https://docs.copilotkit.ai/reference/hooks/useHumanInTheLoop
   */
  useHumanInTheLoop({
    name: "request_approval",
    description: "Request customer approval before processing payment and submitting order",
    parameters: [
      {
        name: "request",
        type: "string",
        description: "The approval request containing function details",
        required: true,
      },
    ],
    render: ({ args, respond, status }) => {
      // Parse the approval request from the wrapper
      let approvalData: { bikeId?: number; amount?: number; approvalId?: string } = {};
      
      if (args.request) {
        try {
          const parsed = typeof args.request === "string" ? JSON.parse(args.request) : args.request;
          console.log("Parsed approval request:", parsed);
          
          // Handle both snake_case (from backend) and PascalCase (legacy)
          const functionArgs = parsed.function_arguments || parsed.FunctionArguments;
          const approvalId = parsed.approval_id || parsed.ApprovalId;
          
          if (functionArgs) {
            approvalData = {
              bikeId: functionArgs.bikeId,
              amount: functionArgs.amount,
              approvalId: approvalId,
            };
          }
        } catch (e) {
          console.error("Failed to parse approval request:", e, "Raw args:", args);
        }
      }
      
      return <ApprovalUI args={approvalData} respond={respond} status={status} />;
    },
  });

  return (
    <div
      className="flex justify-center items-center h-screen w-full bg-gray-50 dark:bg-gray-900"
      data-testid="background-container"
    >
      <div className="h-full w-full md:w-8/10 md:h-8/10 rounded-lg">
        <CopilotChat
          className="h-full rounded-2xl max-w-6xl mx-auto"
          labels={{
            title: "Contoso Bikestore Assistant",
            initial:
              "Welcome to Contoso Bikestore. I'm your personal bike expert assistant.\n\nI can help you:\n• Browse our complete bike catalog\n• Check real-time inventory\n• Get personalized recommendations\n• Learn about bike features and specifications\n\nWhat would you like to know about our bikes today?",
            placeholder: "Ask about bikes, inventory, or recommendations...",
          }}
          suggestions={[
            {
              title: "Browse Bikes",
              message: "Show me all available bikes",
            },
            {
              title: "Check Price",
              message: "How much does the Mountain Explorer Pro cost?",
            },
            {
              title: "Get Recommendation",
              message: "I want a bike for commuting in the city",
            },
          ]}
        />
      </div>
    </div>
  );
};
